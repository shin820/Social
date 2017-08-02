using Framework.Core;
using Social.Domain.DomainServices.Twitter;
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
        private ISocialAccountService _socialAccountService;
        private TweetTreeWalker _twitterTreeWalker;

        public TwitterService(
            IConversationService conversationService,
            IMessageService messageService,
            ISocialUserService socialUserService,
            ISocialAccountService socialAccountService
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _socialUserService = socialUserService;
            _socialAccountService = socialAccountService;
            _twitterTreeWalker = new TweetTreeWalker();
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

        public async Task ProcessTweet(SocialAccount currentAccount, ITweet currentTweet)
        {
            IList<SocialAccount> allAccounts = _socialAccountService.FindAllTwitterAccounts().ToList();

            if (!ShouldProcess(currentAccount, allAccounts, currentTweet))
            {
                return;
            }

            List<ITweet> tweets = _twitterTreeWalker.BuildTweetTree(currentTweet);
            await AddTweets(currentAccount, allAccounts, tweets);
        }

        private bool ShouldProcess(SocialAccount currentAccount, IList<SocialAccount> allAccounts, ITweet currentTweet)
        {
            // ignore if first tweet is sent by integration account.
            if (IsFirstTweetCreateByCurrentIntegrationAccount(currentAccount, currentTweet))
            {
                return false;
            }

            // ignore if first tweet is sent by other integration account.
            if (IsFristTweetCreateByOtherIntegrationAccount(currentAccount, allAccounts, currentTweet))
            {
                return false;
            }

            // ignore if tweet is created by another but not mention me.
            if (IsTweetCreatedByAnotherButDoNotMentionIntegrationAccount(currentAccount, currentTweet))
            {
                return false;
            }

            return true;
        }

        private bool IsFirstTweetCreateByCurrentIntegrationAccount(SocialAccount currentAccount, ITweet currentTweet)
        {
            bool isFirstTweet = currentTweet.InReplyToStatusId == null;
            return isFirstTweet && currentTweet.CreatedBy.IdStr == currentAccount.SocialUser.OriginalId;
        }

        private bool IsFristTweetCreateByOtherIntegrationAccount(SocialAccount currentAccount, IList<SocialAccount> allAccounts, ITweet currentTweet)
        {
            var otherAccountOrginalIds = allAccounts.Where(t => t.Id != currentAccount.Id).Select(t => t.SocialUser.OriginalId);
            bool isFirstTweet = currentTweet.InReplyToStatusId == null;
            return isFirstTweet && otherAccountOrginalIds.Contains(currentTweet.CreatedBy.IdStr);
        }

        private bool IsTweetCreatedByAnotherButDoNotMentionIntegrationAccount(SocialAccount currentAccount, ITweet currentTweet)
        {
            bool isCreatedByAnother = currentTweet.CreatedBy.IdStr != currentAccount.SocialUser.OriginalId;
            bool isMentionMe = currentTweet.UserMentions.Any(t => t.IdStr == currentAccount.SocialUser.OriginalId);
            return isCreatedByAnother && !isMentionMe;
        }

        private async Task AddTweets(SocialAccount currentAccount, IList<SocialAccount> allAccounts, List<ITweet> tweets)
        {
            if (!tweets.Any())
            {
                return;
            }

            tweets = tweets.OrderByDescending(t => t.CreatedAt).ToList();

            foreach (var tweet in tweets)
            {
                // if no intergration account involved.
                if (!IsIntergrationAccountInvolved(tweet, allAccounts))
                {
                    continue;
                }

                await UnitOfWorkManager.RunWithNewTransaction(currentAccount.SiteId, async () =>
                {
                    await AddTweet(currentAccount, allAccounts, tweets, tweet);
                    CurrentUnitOfWork.SaveChanges();
                });
            }
        }

        private bool IsIntergrationAccountInvolved(ITweet tweet, IList<SocialAccount> allAccounts)
        {
            return IsIntegrationAccount(tweet.CreatedBy.IdStr, allAccounts) || tweet.UserMentions.Any(t => IsIntegrationAccount(t.IdStr, allAccounts));
        }

        private async Task AddTweet(SocialAccount account, IList<SocialAccount> socialAccounts, List<ITweet> tweets, ITweet currentTweet)
        {
            var senderOriginalId = currentTweet.CreatedBy.IdStr;
            var receiverOrignalId = GetReceiverOriginalId(account, socialAccounts, currentTweet);
            if (string.IsNullOrEmpty(receiverOrignalId))
            {
                return;
            }
            // integration account sent to intergration account
            if (IsIntegrationAccount(senderOriginalId, socialAccounts)
                && IsIntegrationAccount(receiverOrignalId, socialAccounts))
            {
                List<string> customerOriginalIds = _twitterTreeWalker.FindCustomerOriginalIdsInTweetTree(currentTweet, tweets, socialAccounts);
                if (!customerOriginalIds.Any())
                {
                    return;
                }

                await SaveTweet(account, currentTweet, tweets, senderOriginalId, receiverOrignalId, customerOriginalIds);
            }

            // integration account sent to customer
            if (IsIntegrationAccount(senderOriginalId, socialAccounts)
                && !IsIntegrationAccount(receiverOrignalId, socialAccounts))
            {
                var customerOriginalIds = new List<string> { receiverOrignalId };
                await SaveTweet(account, currentTweet, tweets, senderOriginalId, receiverOrignalId, customerOriginalIds);
            }

            // customer sent to integration account
            if (!IsIntegrationAccount(senderOriginalId, socialAccounts)
                && IsIntegrationAccount(receiverOrignalId, socialAccounts))
            {
                var customerOriginalIds = new List<string> { senderOriginalId };
                await SaveTweet(account, currentTweet, tweets, senderOriginalId, receiverOrignalId, customerOriginalIds);
            }
        }

        private async Task SaveTweet(SocialAccount account, ITweet currentTweet, List<ITweet> tweets, string senderOriginalId, string receiverOriginalId, List<string> customerOrginalIds)
        {
            var sender = await _socialUserService.GetOrCreateTwitterUser(senderOriginalId);
            var receiver = await _socialUserService.GetOrCreateTwitterUser(receiverOriginalId);
            var conversations = FindConversations(tweets, customerOrginalIds);
            var message = ConvertToMessage(currentTweet);
            message.SenderId = sender.Id;
            message.ReceiverId = receiver.Id;
            SaveConversations(account, conversations, message);
        }

        private IList<Conversation> FindConversations(IList<ITweet> tweets, List<string> customerOriginalIds)
        {
            List<string> tweetIds = tweets.Select(t => t.IdStr).ToList();
            return _conversationService.FindAll().Include(t => t.Messages)
                  .Where(c => c.Messages.Any(m => tweetIds.Contains(m.OriginalId)
                  && (customerOriginalIds.Contains(m.Sender.OriginalId) || customerOriginalIds.Contains(m.Receiver.OriginalId)))
                  ).ToList();
        }

        private string GetReceiverOriginalId(SocialAccount account, IList<SocialAccount> socialAccounts, ITweet currentTweet)
        {
            List<string> accountOrignalIds = socialAccounts.Select(t => t.SocialUser.OriginalId).ToList();

            // if first tweet sent by integraton account and didn't mentioned any one, 
            // the recipient should be the integraiton account himself.
            if (!currentTweet.UserMentions.Any())
            {
                return account.SocialUser.OriginalId;
            }

            // if tweet sent by customer, the reciver should be the first integration account in the user mentions.
            if (!IsIntegrationAccount(currentTweet.CreatedBy.IdStr, socialAccounts))
            {
                return currentTweet.UserMentions.Where(t => accountOrignalIds.Contains(t.IdStr))
                    .Select(t => t.IdStr).FirstOrDefault();
            }
            // if tweet sent by integration account, the reciver should either be customer or the first integration account in the user mentions.
            else
            {
                var receiverOriginalId = currentTweet.UserMentions.Where(t => !accountOrignalIds.Contains(t.IdStr))
                    .Select(t => t.IdStr).FirstOrDefault();
                if (!string.IsNullOrEmpty(receiverOriginalId))
                {
                    return receiverOriginalId;
                }

                return currentTweet.UserMentions.Select(t => t.IdStr).FirstOrDefault();
            }
        }

        private bool IsIntegrationAccount(string originalId, IList<SocialAccount> socialAccounts)
        {
            return socialAccounts.Any(t => t.SocialUser.OriginalId == originalId);
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

            if (media.VideoDetails != null && media.VideoDetails.Variants.Any())
            {
                var video = media.VideoDetails.Variants.FirstOrDefault();

                return new MessageAttachment
                {
                    Type = type,
                    Url = video.URL,
                    PreviewUrl = media.MediaURL,
                    MimeType = new Uri(video.URL).GetMimeType(),
                    OriginalId = media.IdStr,
                    OriginalLink = media.URL
                };
            }

            return new MessageAttachment
            {
                Type = type,
                Url = media.MediaURL,
                PreviewUrl = media.MediaURL,
                MimeType = new Uri(media.MediaURL).GetMimeType(),
                OriginalId = media.IdStr,
                OriginalLink = media.URL
            };
        }

    }
}
