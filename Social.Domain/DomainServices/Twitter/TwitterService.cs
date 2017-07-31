using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;
using Tweetinvi.Parameters;

namespace Social.Domain.DomainServices
{
    public interface ITwitterService
    {
        Task ProcessDirectMessage(SocialAccount account, IMessage directMsg);
        Task ProcessTweet(SocialAccount account, ITweet currentTweet);
        Entities.Message GetTweetMessage(SocialAccount socialAccount, long tweetId);
        ITweet GetTweet(SocialAccount socialAccount, long tweetId);
        ITweet ReplyTweet(SocialAccount socialAccount, ITweet inReplyTo, string message);
        Entities.Message ConvertToMessage(ITweet tweet);
        Entities.Message ConvertToMessage(IMessage directMessage);
        IUser GetUser(SocialAccount socialAccount, long userId);
        IMessage PublishMessage(SocialAccount socialAccount, IUser user, string message);
    }

    public class TwitterService : ServiceBase, ITwitterService
    {
        private IConversationService _conversationService;
        private IMessageService _messageService;
        private ISocialUserService _socialUserService;

        public TwitterService(
            IConversationService conversationService,
            IMessageService messageService,
            ISocialUserService socialUserService
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _socialUserService = socialUserService;
        }


        public Entities.Message GetTweetMessage(SocialAccount account, long tweetId)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, account.Token, account.TokenSecret));
            var tweet = Tweet.GetTweet(tweetId);
            if (tweet == null)
            {
                return null;
            }

            return ConvertToMessage(tweet);
        }

        public ITweet GetTweet(SocialAccount socialAccount, long tweetId)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, socialAccount.Token, socialAccount.TokenSecret));

            return Tweet.GetTweet(tweetId);
        }

        public IUser GetUser(SocialAccount socialAccount, long userId)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, socialAccount.Token, socialAccount.TokenSecret));
            return User.GetUserFromId(userId);
        }

        public ITweet ReplyTweet(SocialAccount socialAccount, ITweet inReplyTo, string message)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, socialAccount.Token, socialAccount.TokenSecret));

            return Tweet.PublishTweet(message, new PublishTweetOptionalParameters
            {
                InReplyToTweet = inReplyTo
            });
        }

        public IMessage PublishMessage(SocialAccount socialAccount, IUser user, string message)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, socialAccount.Token, socialAccount.TokenSecret));

            return Tweetinvi.Message.PublishMessage(message, user);
        }

        public async Task ProcessDirectMessage(SocialAccount account, IMessage directMsg)
        {
            if (_messageService.IsDuplicatedMessage(MessageSource.TwitterDirectMessage, directMsg.Id.ToString()))
            {
                return;
            }

            bool isSendByAccount = directMsg.SenderId.ToString() == account.SocialUser.OriginalId;
            SocialUser sender = await _socialUserService.GetOrCreateTwitterUser(directMsg.Sender);
            SocialUser recipient = await _socialUserService.GetOrCreateTwitterUser(directMsg.Recipient);
            var existingConversation = _conversationService.GetTwitterDirectMessageConversation(sender, recipient);
            if (existingConversation != null)
            {
                var message = ConvertToMessage(directMsg);
                message.SenderId = sender.Id;
                message.ReceiverId = recipient.Id;
                message.ConversationId = existingConversation.Id;
                existingConversation.IfRead = false;
                bool isSendByIntegrationAccount = sender.Id == account.Id & recipient.Id != account.Id;
                existingConversation.Status = isSendByIntegrationAccount ? ConversationStatus.PendingExternal : ConversationStatus.PendingInternal;
                existingConversation.LastMessageSenderId = message.SenderId;
                existingConversation.LastMessageSentTime = message.SendTime;
                existingConversation.Messages.Add(message);
                _conversationService.Update(existingConversation);
            }
            else
            {
                if (sender.Id == account.SocialUser.Id)
                {
                    return;
                }

                var message = ConvertToMessage(directMsg);
                message.SenderId = sender.Id;
                message.ReceiverId = recipient.Id;
                var conversation = new Conversation
                {
                    OriginalId = directMsg.Id.ToString(),
                    Source = ConversationSource.TwitterDirectMessage,
                    Priority = ConversationPriority.Normal,
                    Status = ConversationStatus.New,
                    Subject = GetSubject(message.Content),
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };
                conversation.Messages.Add(message);
                _conversationService.AddConversation(account, conversation);
            }
        }

        public Entities.Message ConvertToMessage(IMessage directMsg)
        {
            var message = new Entities.Message
            {
                Source = MessageSource.TwitterDirectMessage,
                Content = directMsg.Text,
                OriginalId = directMsg.Id.ToString(),
                SendTime = directMsg.CreatedAt.ToUniversalTime()
            };

            return message;
        }

        public async Task ProcessTweet(SocialAccount account, ITweet currentTweet)
        {
            if (!ShouldProcess(account, currentTweet))
            {
                return;
            }

            List<ITweet> tweets = new List<ITweet>();

            RecursivelyFillTweet(tweets, currentTweet);

            bool ifAllTweetsCreateByAccount = tweets.All(t => t.CreatedBy.IdStr == account.SocialUser.OriginalId);
            if (ifAllTweetsCreateByAccount)
            {
                return;
            }

            await AddTweets(account, tweets);
        }

        private bool ShouldProcess(SocialAccount account, ITweet currentTweet)
        {
            // ignore if typical tweet is sent by integration account.
            if (IsTypicalTweetCreateByIntegrationAccount(account, currentTweet))
            {
                return false;
            }

            // ignore if tweet is created by another but not mention me.
            if (IsTweetCreatedByAnotherButDoNotMentionIntegrationAccount(account, currentTweet))
            {
                return false;
            }

            return true;
        }

        private bool IsTypicalTweetCreateByIntegrationAccount(SocialAccount account, ITweet currentTweet)
        {
            bool isFirstTweetSendByIntegrationAccount = currentTweet.InReplyToStatusId == null
                 && currentTweet.CreatedBy.IdStr == account.SocialUser.OriginalId;
            return isFirstTweetSendByIntegrationAccount;
        }

        private bool IsTweetCreatedByAnotherButDoNotMentionIntegrationAccount(SocialAccount account, ITweet currentTweet)
        {
            bool isCreatedByAnother = currentTweet.CreatedBy.IdStr != account.SocialUser.OriginalId;
            bool isMentionMe = currentTweet.UserMentions.Any(t => t.IdStr == account.SocialUser.OriginalId);
            return isCreatedByAnother && !isMentionMe;
        }

        private async Task AddTweets(SocialAccount account, List<ITweet> tweets)
        {
            if (!tweets.Any())
            {
                return;
            }

            tweets = tweets.OrderByDescending(t => t.CreatedAt).ToList();
            var socialAccounts = _socialUserService.FindAll()
                .Where(t => t.Type == SocialUserType.IntegrationAccount && t.Source == SocialUserSource.Twitter)
                .ToList();

            foreach (var tweet in tweets)
            {
                await UnitOfWorkManager.RunWithNewTransaction(account.SiteId, async () =>
                {
                    await AddTweet(account, socialAccounts, tweets, tweet);
                    CurrentUnitOfWork.SaveChanges();
                });
            }
        }

        private async Task AddTweet(SocialAccount account, List<SocialUser> socialAccounts, List<ITweet> tweets, ITweet currentTweet)
        {
            var tweetIds = tweets.Select(t => t.IdStr);
            var currentTweetSender = currentTweet.CreatedBy.IdStr;
            var currentTweetRecipient = currentTweet.InReplyToStatusId != null ? currentTweet.InReplyToUserIdStr : account.SocialUser.OriginalId;

            // if a new customer replied customer and mentioned integration account , the recipient should be integration account.
            if (!socialAccounts.Any(t => t.OriginalId.ToString() == currentTweetSender) && !socialAccounts.Any(t => t.OriginalId.ToString() == currentTweetRecipient))
            {
                currentTweetRecipient = account.SocialUser.OriginalId;
            }

            // integration account sent to intergration account
            if (socialAccounts.Any(t => t.OriginalId.ToString() == currentTweetSender)
                && socialAccounts.Any(t => t.OriginalId.ToString() == currentTweetRecipient))
            {
                var conversations = _conversationService.FindAll().Include(t => t.Messages)
                    .Where(c =>
                    c.Messages.Any(m => tweetIds.Contains(m.OriginalId) &&
                    (m.SenderId == account.Id || m.ReceiverId == account.Id))
                    ).ToList();
                var message = ConvertToMessage(currentTweet);
                message.SenderId = account.Id;
                message.ReceiverId = account.Id;
                SaveConversations(account, conversations, message);
            }

            // integration account sent to customer
            if (socialAccounts.Any(t => t.OriginalId.ToString() == currentTweetSender)
                && !socialAccounts.Any(t => t.OriginalId.ToString() == currentTweetRecipient))
            {
                var inReplyToUser = tweets.FirstOrDefault(t => t.Id == currentTweet.InReplyToStatusId)?.CreatedBy;
                if (inReplyToUser == null)
                {
                    inReplyToUser = User.GetUserFromId(long.Parse(currentTweetRecipient));
                }

                var socialAccountIds = socialAccounts.Select(t => t.Id).ToList();
                var recipient = await _socialUserService.GetOrCreateTwitterUser(inReplyToUser);
                var conversations = _conversationService.FindAll().Include(t => t.Messages)
                    .Where(c => c.Messages.Any(m => tweetIds.Contains(m.OriginalId)
                     && (m.SenderId == recipient.Id || m.ReceiverId == recipient.Id))
                    ).ToList();
                var message = ConvertToMessage(currentTweet);
                message.SenderId = account.Id;
                message.ReceiverId = recipient.Id;
                SaveConversations(account, conversations, message);
            }

            // customer sent to integration account
            if (!socialAccounts.Any(t => t.OriginalId.ToString() == currentTweetSender)
                && socialAccounts.Any(t => t.OriginalId.ToString() == currentTweetRecipient))
            {
                var socialAccountIds = socialAccounts.Select(t => t.Id).ToList();
                var sender = await _socialUserService.GetOrCreateTwitterUser(currentTweet.CreatedBy);
                var conversations = _conversationService.FindAll().Include(t => t.Messages)
                    .Where(c => c.Messages.Any(m => tweetIds.Contains(m.OriginalId)
                    && (m.SenderId == sender.Id || m.ReceiverId == sender.Id))
                    ).ToList();
                var message = ConvertToMessage(currentTweet);
                message.SenderId = sender.Id;
                message.ReceiverId = account.Id;
                SaveConversations(account, conversations, message);
            }
        }

        private void SaveConversations(SocialAccount account, IList<Conversation> conversations, Entities.Message message)
        {
            if (conversations.Any())
            {
                foreach (var conversation in conversations)
                {
                    if (conversation.Messages.Any(t => t.OriginalId == message.OriginalId))
                    {
                        continue;
                    }
                    conversation.Messages.Add(message);
                    if (message.SendTime > conversation.LastMessageSentTime)
                    {
                        conversation.IfRead = false;
                        conversation.Status = message.SenderId == account.Id ? ConversationStatus.PendingExternal : ConversationStatus.PendingInternal;
                        conversation.LastMessageSenderId = message.SenderId;
                        conversation.LastMessageSentTime = message.SendTime;
                    }
                    _conversationService.Update(conversation);
                }
            }
            else
            {
                var conversation = new Conversation
                {
                    OriginalId = message.OriginalId,
                    Source = ConversationSource.TwitterTweet,
                    Priority = ConversationPriority.Normal,
                    Status = ConversationStatus.New,
                    Subject = GetSubject(message.Content),
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };
                conversation.Messages.Add(message);
                _conversationService.AddConversation(account, conversation);
            }
        }

        private void RecursivelyFillTweet(IList<ITweet> tweets, ITweet tweet)
        {
            tweets.Add(tweet);

            // for performance reason, we just get 10 parent tweet in the tree.
            if (tweets.Count >= 10)
            {
                return;
            }

            if (tweet.InReplyToStatusId != null)
            {
                ITweet inReplyToTweet = Tweet.GetTweet(tweet.InReplyToStatusId.Value);
                if (inReplyToTweet == null)
                {
                    return;
                }
                RecursivelyFillTweet(tweets, inReplyToTweet);
            }
        }

        private string GetSubject(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "No Subject";
            }

            return message.Length <= 200 ? message : message.Substring(200);
        }

        public Entities.Message ConvertToMessage(ITweet tweet)
        {
            var message = new Entities.Message
            {
                Source = tweet.QuotedStatusId == null ? MessageSource.TwitterTypicalTweet : MessageSource.TwitterQuoteTweet,
                OriginalId = tweet.IdStr,
                SendTime = tweet.CreatedAt.ToUniversalTime(),
                Content = string.IsNullOrWhiteSpace(tweet.Text) ? tweet.FullText : tweet.Text,
                OriginalLink = tweet.Url
            };
            if (tweet.QuotedStatusId != null)
            {
                message.QuoteTweetId = tweet.QuotedStatusIdStr;
            }

            if (tweet.Media != null)
            {
                foreach (var media in tweet.Media)
                {
                    message.Attachments.Add(ConvertToMessageAttachment(media));
                }
            }

            return message;
        }

        private MessageAttachment ConvertToMessageAttachment(IMediaEntity media)
        {
            MessageAttachmentType type = MessageAttachmentType.File;
            if (media.MediaType == "animated_gif")
            {
                type = MessageAttachmentType.AnimatedImage;
            }
            if (media.MediaType == "photo")
            {
                type = MessageAttachmentType.Image;
            }
            if (media.MediaType == "vedio")
            {
                type = MessageAttachmentType.Video;
            }

            return new MessageAttachment
            {
                Type = type,
                Url = media.URL,
                PreviewUrl = media.MediaURL,
                OriginalId = media.IdStr,
                OriginalLink = media.URL
            };
        }

    }
}
