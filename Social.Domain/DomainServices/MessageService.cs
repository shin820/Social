using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tweetinvi.Models;
using Message = Social.Domain.Entities.Message;

namespace Social.Domain.DomainServices
{
    public interface IMessageService : IDomainService<Message>
    {
        Message FindByOriginalId(MessageSource source, string originalId);
        IQueryable<Message> FindAllByConversationId(int conversationId);
        Message GetTwitterTweetMessage(string originalId);
        bool IsDuplicatedMessage(MessageSource messageSource, string originalId);
        bool IsDupliatedTweet(ITweet tweet);
        Message ReplyTwitterTweetMessage(int conversationId, int twitterAccountId, int parentId, string message);
        Message ReplyTwitterDirectMessage(int conversationId, int twitterAccountId, string message);
        Message ReplyFacebookMessage(int conversationId, string content);
        Message ReplyFacebookPostOrComment(int conversationId, int parentId, string content);
    }

    public class MessageService : DomainService<Message>, IMessageService
    {
        private ISocialAccountService _socialAccountService;
        private IConversationService _conversationService;
        public MessageService(
            ISocialAccountService socialAccountService,
            IConversationService conversationService
            )
        {
            _socialAccountService = socialAccountService;
            _conversationService = conversationService;
        }

        public Message FindByOriginalId(MessageSource source, string originalId)
        {
            return Repository.FindAll().Where(t => t.OriginalId == originalId && t.Source == source && t.IsDeleted == false).FirstOrDefault();
        }

        public IQueryable<Message> FindAllByConversationId(int conversationId)
        {
            return Repository.FindAll()
                .Include(t => t.Attachments)
                .Include(t => t.Sender.SocialAccount)
                .Include(t => t.Receiver.SocialAccount)
                .Where(t => t.ConversationId == conversationId && t.IsDeleted == false);
        }

        private IQueryable<Message> FindAllInlcudeDeletedByConversationId(int conversationId)
        {
            return Repository.FindAll()
                .Include(t => t.Attachments)
                .Include(t => t.Sender.SocialAccount)
                .Include(t => t.Receiver.SocialAccount)
                .Where(t => t.ConversationId == conversationId);
        }

        public Message ReplyFacebookMessage(int conversationId, string content)
        {
            var conversation = _conversationService.CheckIfExists(conversationId);
            if (conversation.Source != ConversationSource.FacebookMessage)
            {
                throw SocialExceptions.BadRequest("Conversation source must be facebook message.");
            }

            var messages = FindAllByConversationId(conversation.Id).ToList();
            SocialAccount socialAccount = GetSocialAccountsFromMessages(messages).FirstOrDefault();
            if (socialAccount == null)
            {
                throw SocialExceptions.BadRequest("Facebook integration account can't be found from conversation.");
            }

            SocialUser recipient = messages.Where(t => t.Sender.Type == SocialUserType.Customer)
                .OrderByDescending(t => t.SendTime)
                .Select(t => t.Sender).FirstOrDefault();

            // publish message to facebook
            string fbMessageId = FbClient.PublishMessage(socialAccount.Token, conversation.OriginalId, content);

            // create message
            var message = new Message
            {
                ConversationId = conversation.Id,
                Content = content,
                SenderId = socialAccount.Id,
                ReceiverId = recipient != null ? recipient.Id : socialAccount.Id,
                SendAgentId = UserContext.UserId,
                SendTime = DateTime.UtcNow,
                OriginalId = fbMessageId,
                Source = MessageSource.FacebookMessage
            };
            Repository.Insert(message);
            CurrentUnitOfWork.SaveChanges();

            // upadte conversation
            conversation.Status = ConversationStatus.PendingExternal;
            conversation.LastMessageSenderId = message.SenderId;
            conversation.LastMessageSentTime = message.SendTime;
            conversation.LastRepliedAgentId = UserContext.UserId;
            _conversationService.Update(conversation);

            return message;
        }

        public Message ReplyFacebookPostOrComment(int conversationId, int parentId, string content)
        {
            var conversation = _conversationService.CheckIfExists(conversationId);
            if (conversation.Source != ConversationSource.FacebookVisitorPost && conversation.Source != ConversationSource.FacebookWallPost)
            {
                throw SocialExceptions.BadRequest("Conversation source must be facebook visitor/wall post.");
            }

            var messages = FindAllInlcudeDeletedByConversationId(conversation.Id).ToList();
            SocialAccount socialAccount = GetSocialAccountsFromMessages(messages).FirstOrDefault();
            if (socialAccount == null)
            {
                throw SocialExceptions.BadRequest("Facebook integration account can't be found from conversation.");
            }

            var previousMessages = GetPreviousMessages(messages, parentId);
            Message comment = null;
            foreach (var previousMessage in previousMessages)
            {
                if (previousMessage.IsDeleted)
                {
                    continue;
                }

                // publish comment
                string fbCommentId = FbClient.PublishComment(socialAccount.Token, previousMessage.OriginalId, content);
                if (string.IsNullOrWhiteSpace(fbCommentId))
                {
                    continue;
                }

                var fbComment = FbClient.GetComment(socialAccount.Token, fbCommentId);

                // add message
                comment = new Message
                {
                    ConversationId = conversation.Id,
                    Content = content,
                    SenderId = socialAccount.Id,
                    ReceiverId = previousMessage.SenderId,
                    ParentId = previousMessage.Id,
                    SendAgentId = UserContext.UserId,
                    SendTime = fbComment.created_time,
                    OriginalId = fbCommentId,
                    OriginalLink = fbComment.permalink_url,
                    Source = MessageSource.FacebookPostComment
                };
                Repository.Insert(comment);
                CurrentUnitOfWork.SaveChanges();

                // upadte conversation
                conversation.Status = ConversationStatus.PendingExternal;
                conversation.LastMessageSenderId = comment.SenderId;
                conversation.LastMessageSentTime = comment.SendTime;
                conversation.LastRepliedAgentId = UserContext.UserId;
                _conversationService.Update(conversation);

                break;
            }

            if (comment == null)
            {
                throw SocialExceptions.OriginalPostOrTweetHasBeenDeleted();
            }

            return comment;
        }

        public Message ReplyTwitterDirectMessage(int conversationId, int twitterAccountId, string message)
        {
            var twitterService = DependencyResolver.Resolve<ITwitterService>();

            Conversation conversation = _conversationService.CheckIfExists(conversationId);
            if (conversation.Source != ConversationSource.TwitterDirectMessage)
            {
                throw SocialExceptions.BadRequest("Conversation source must be twitter direct message.");
            }

            SocialAccount twitterAccount = _socialAccountService.Find(twitterAccountId);
            if (twitterAccount == null)
            {
                throw SocialExceptions.BadRequest("Invalid twitter account id.");
            }

            var messages = FindAllByConversationId(conversation.Id).ToList();
            var lastMessageSender = messages.Where(t => t.SenderId != twitterAccountId).OrderByDescending(t => t.SendTime).Select(t => t.Sender).FirstOrDefault();
            if (lastMessageSender == null)
            {
                throw SocialExceptions.BadRequest("Cant't find last message from conversation.");
            }

            IUser prviousTwitterUser = twitterService.GetUser(twitterAccount, long.Parse(lastMessageSender.OriginalId));
            if (prviousTwitterUser == null)
            {
                throw SocialExceptions.BadRequest("Cant't find twitter user.");
            }

            // publish twitter direct message
            IMessage twitterDirectMessage = twitterService.PublishMessage(twitterAccount, prviousTwitterUser, message);

            // create message to db
            Message directMessage = twitterService.ConvertToMessage(twitterDirectMessage);
            directMessage.ConversationId = conversation.Id;
            directMessage.SenderId = twitterAccount.Id;
            directMessage.SendAgentId = UserContext.UserId;
            directMessage.ReceiverId = lastMessageSender.Id;
            Repository.Insert(directMessage);
            CurrentUnitOfWork.SaveChanges();

            // upadte conversation
            conversation.Status = ConversationStatus.PendingExternal;
            conversation.LastMessageSenderId = twitterAccount.Id;
            conversation.LastMessageSentTime = directMessage.SendTime;
            conversation.LastRepliedAgentId = UserContext.UserId;
            _conversationService.Update(conversation);

            return directMessage;
        }

        public Message ReplyTwitterTweetMessage(int conversationId, int twitterAccountId, int parentId, string content)
        {
            var twitterService = DependencyResolver.Resolve<ITwitterService>();
            Conversation conversation = _conversationService.CheckIfExists(conversationId);
            if (conversation.Source != ConversationSource.TwitterTweet)
            {
                throw SocialExceptions.BadRequest("Conversation source must be twitter tweet.");
            }

            SocialAccount twitterAccount = _socialAccountService.Find(twitterAccountId);
            if (twitterAccount == null)
            {
                throw SocialExceptions.BadRequest("Invalid twitter account id.");
            }

            var messages = FindAllInlcudeDeletedByConversationId(conversation.Id).ToList();
            var previousMessages = GetPreviousMessages(messages, parentId);
            Message replyMessage = null;
            foreach (var previousMessage in previousMessages)
            {
                if (previousMessage.IsDeleted)
                {
                    continue;
                }
                //if (previousMessage.Sender.Type == SocialUserType.IntegrationAccount)
                //{
                //    continue;
                //}

                ITweet previousTweet = twitterService.GetTweet(twitterAccount, long.Parse(previousMessage.OriginalId));
                if (previousTweet != null && !previousTweet.IsTweetDestroyed)
                {
                    // publish tweet
                    if (!content.Contains("@" + previousTweet.CreatedBy.ScreenName))
                    {
                        content = "@" + previousTweet.CreatedBy.ScreenName + " " + content;
                    }
                    var replyTweet = twitterService.ReplyTweet(twitterAccount, previousTweet, content);
                    if (replyTweet == null)
                    {
                        continue;
                    }

                    // add message
                    replyMessage = twitterService.ConvertToMessage(replyTweet);
                    replyMessage.ConversationId = conversation.Id;
                    replyMessage.SenderId = twitterAccount.Id;
                    replyMessage.SendAgentId = UserContext.UserId;
                    replyMessage.ReceiverId = previousMessage.Sender.Id;
                    replyMessage.ParentId = previousMessage.Id;
                    Repository.Insert(replyMessage);
                    CurrentUnitOfWork.SaveChanges();

                    // upadte conversation
                    conversation.Status = ConversationStatus.PendingExternal;
                    conversation.LastMessageSenderId = twitterAccount.Id;
                    conversation.LastMessageSentTime = replyMessage.SendTime;
                    conversation.LastRepliedAgentId = UserContext.UserId;
                    _conversationService.Update(conversation);
                    break;
                }
            }

            if (replyMessage == null)
            {
                throw SocialExceptions.OriginalPostOrTweetHasBeenDeleted();
            }

            return replyMessage;
        }

        public Message GetTwitterTweetMessage(string originalId)
        {
            return Repository.FindAll()
                .Where(t => t.OriginalId == originalId && (t.Source == MessageSource.TwitterTypicalTweet || t.Source == MessageSource.TwitterQuoteTweet))
                .FirstOrDefault();
        }

        public bool IsDuplicatedMessage(MessageSource messageSource, string originalId)
        {
            return Repository.FindAll().Any(t => t.Source == messageSource && t.OriginalId == originalId);
        }

        public bool IsDupliatedTweet(ITweet tweet)
        {
            bool isQuoteTweet = tweet.QuotedTweet != null;

            if (isQuoteTweet)
            {
                return this.IsDuplicatedMessage(MessageSource.TwitterQuoteTweet, tweet.IdStr);
            }
            else
            {
                return this.IsDuplicatedMessage(MessageSource.TwitterTypicalTweet, tweet.IdStr);
            }
        }

        private IList<SocialAccount> GetSocialAccountsFromMessages(IList<Message> messages)
        {
            var accountsWhichSendMessage =
                messages.Where(t => t.Sender.Type == SocialUserType.IntegrationAccount)
                .Select(t => t.Sender.SocialAccount)
                .Distinct()
                .ToList();
            var accountsWhichReceiveMessage =
                messages.Where(t => t.Receiver != null && t.Receiver.Type == SocialUserType.IntegrationAccount)
                .Select(t => t.Receiver.SocialAccount)
                .Distinct()
                .ToList();
            List<SocialAccount> accounts = new List<SocialAccount>();
            foreach (var accountWithSendMessage in accountsWhichSendMessage)
            {
                if (accounts.Any(t => t.Id == accountWithSendMessage.Id))
                {
                    continue;
                }
                accounts.Add(accountWithSendMessage);
            }

            foreach (var accountWhichReceiveMessage in accountsWhichReceiveMessage)
            {
                if (accounts.Any(t => t.Id == accountWhichReceiveMessage.Id))
                {
                    continue;
                }
                accounts.Add(accountWhichReceiveMessage);
            }

            return accounts;
        }

        private IList<Message> GetPreviousMessages(IList<Message> messages, int previousMessageId)
        {
            List<Message> previousMessages = new List<Message>();
            var prviousMessage = messages.FirstOrDefault(t => t.Id == previousMessageId);
            while (prviousMessage != null)
            {
                previousMessages.Add(prviousMessage);
                if (prviousMessage.ParentId != null)
                {
                    prviousMessage = messages.FirstOrDefault(t => t.Id == prviousMessage.ParentId);
                }
                else
                {
                    break;
                }
            }

            return previousMessages.OrderByDescending(t => t.SendTime).ToList();
        }
    }
}
