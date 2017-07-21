using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Message = Social.Domain.Entities.Message;

namespace Social.Domain.DomainServices
{
    public interface IMessageService : IDomainService<Message>
    {
        IQueryable<Message> FindAllByConversationId(int conversationId);
        Message GetTwitterTweetMessage(string originalId);
        bool IsDuplicatedMessage(MessageSource messageSource, string originalId);
        bool IsDupliatedTweet(ITweet tweet);
        Message ReplyTweetMessage(int conversationId, int twitterAccountId, string message);
        Message ReplyTweetDirectMessage(int conversationId, int twitterAccountId, string message);
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

        public IQueryable<Message> FindAllByConversationId(int conversationId)
        {
            return Repository.FindAll()
                .Include(t => t.Attachments)
                .Include(t => t.Sender)
                .Where(t => t.ConversationId == conversationId && t.IsDeleted == false);
        }

        public Message ReplyFacebookMessage(int conversationId, string content)
        {
            var conversation = _conversationService.CheckIfExists(conversationId);
            SocialAccount socialAccount = conversation.Messages.Where(t => t.Sender.SocialAccount != null).Select(t => t.Sender.SocialAccount).FirstOrDefault();
            if (socialAccount == null)
            {
                socialAccount = conversation.Messages.Where(t => t.Receiver.SocialAccount != null).Select(t => t.Receiver.SocialAccount).FirstOrDefault();
            }
            if (socialAccount == null)
            {
                throw new BadRequestException("Facebook social account can't be found.");
            }

            SocialUser recipient = conversation.Messages.Where(t => t.Sender.SocialAccount == null).OrderByDescending(t => t.SendTime).Select(t => t.Sender).FirstOrDefault();

            // add message
            string fbMessageId = FbClient.PublishMessage(socialAccount.Token, conversation.OriginalId, content);
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
            SocialAccount socialAccount = conversation.Messages.Where(t => t.Sender.SocialAccount != null).Select(t => t.Sender.SocialAccount).FirstOrDefault();
            if (socialAccount == null)
            {
                socialAccount = conversation.Messages.Where(t => t.Receiver.SocialAccount != null).Select(t => t.Receiver.SocialAccount).FirstOrDefault();
            }
            if (socialAccount == null)
            {
                throw new BadRequestException("Facebook social account can't be found.");
            }

            var parentMessage = conversation.Messages.FirstOrDefault(t => t.Id == parentId);
            if (parentMessage == null)
            {
                throw new BadRequestException("Invalid parent id.");
            }

            // add message
            string fbCommentId = FbClient.PublishComment(socialAccount.Token, parentMessage.OriginalId, content);
            var fbComment = FbClient.GetComment(socialAccount.Token, fbCommentId);
            var message = new Message
            {
                ConversationId = conversation.Id,
                Content = content,
                SenderId = socialAccount.Id,
                ReceiverId = parentMessage.SenderId,
                SendAgentId = UserContext.UserId,
                SendTime = fbComment.created_time,
                OriginalId = fbCommentId,
                OriginalLink = fbComment.permalink_url,
                Source = MessageSource.FacebookPostComment
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

        public Message ReplyTweetDirectMessage(int conversationId, int twitterAccountId, string message)
        {
            var twitterService = DependencyResolver.Resolve<ITwitterService>();

            Conversation conversation = _conversationService.CheckIfExists(conversationId);

            var previousMessages = FindAllByConversationId(conversationId)
                .Where(t => t.SenderId != twitterAccountId)
                .OrderByDescending(t => t.SendTime);

            SocialAccount twitterAccount = _socialAccountService.Find(twitterAccountId);
            if (twitterAccount == null)
            {
                throw new BadRequestException("Invalid twitter account id.");
            }

            Message directMessage = null;
            if (previousMessages.Any())
            {
                foreach (var previousMessage in previousMessages)
                {
                    IUser prviousUser = twitterService.GetUser(twitterAccount, long.Parse(previousMessage.Sender.OriginalId));
                    if (prviousUser != null)
                    {
                        // add tweet message
                        IMessage twitterDirectMessage = twitterService.PublishMessage(twitterAccount, prviousUser, message);
                        directMessage = twitterService.ConvertToMessage(twitterDirectMessage);
                        directMessage.ConversationId = conversation.Id;
                        directMessage.SenderId = twitterAccount.Id;
                        directMessage.SendAgentId = UserContext.UserId;
                        directMessage.ReceiverId = previousMessage.Sender.Id;
                        Repository.Insert(directMessage);
                        CurrentUnitOfWork.SaveChanges();

                        // upadte conversation
                        conversation.Status = ConversationStatus.PendingExternal;
                        conversation.LastMessageSenderId = twitterAccount.Id;
                        conversation.LastMessageSentTime = directMessage.SendTime;
                        conversation.LastRepliedAgentId = UserContext.UserId;
                        _conversationService.Update(conversation);
                        break;
                    }
                }
            }

            if (directMessage == null)
            {
                throw new BadRequestException("There is no tweet message to reply.");
            }

            return directMessage;
        }

        public Message ReplyTweetMessage(int conversationId, int twitterAccountId, string message)
        {
            var twitterService = DependencyResolver.Resolve<ITwitterService>();
            Conversation conversation = _conversationService.CheckIfExists(conversationId);

            var previousMessages = FindAllByConversationId(conversationId)
                .Where(t => t.SenderId != twitterAccountId)
                .OrderByDescending(t => t.SendTime)
                .ToList();

            SocialAccount twitterAccount = _socialAccountService.Find(twitterAccountId);
            if (twitterAccount == null)
            {
                throw new BadRequestException("Invalid twitter account id.");
            }

            Message replyMessage = null;
            if (previousMessages.Any())
            {
                foreach (var previousMessage in previousMessages)
                {
                    ITweet previousTweet = twitterService.GetTweet(twitterAccount, long.Parse(previousMessage.OriginalId));
                    if (previousTweet != null && !previousTweet.IsTweetDestroyed)
                    {
                        // add tweet message
                        var replyTweet = twitterService.ReplyTweet(twitterAccount, previousTweet, message);
                        replyMessage = twitterService.ConvertToMessage(replyTweet);
                        replyMessage.ConversationId = conversation.Id;
                        replyMessage.SenderId = twitterAccount.Id;
                        replyMessage.SendAgentId = UserContext.UserId;
                        replyMessage.ReceiverId = previousMessage.Sender.Id;
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
            }

            if (replyMessage == null)
            {
                throw new BadRequestException("There is no tweet message to reply.");
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
    }
}
