using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices.Twitter
{
    public interface ITwitterTweetService
    {
        Task<TwitterProcessResult> ProcessTweet(SocialAccount currentAccount, ITweet currentTweet);
    }

    public class TwitterTweetService : ServiceBase, ITwitterTweetService
    {
        private IConversationService _conversationService;
        private IMessageService _messageService;
        private ISocialUserService _socialUserService;
        private ISocialAccountService _socialAccountService;
        private TweetTreeWalker _twitterTreeWalker;
        private INotificationManager _notificationManager;

        public TwitterTweetService(
            IConversationService conversationService,
            IMessageService messageService,
            ISocialUserService socialUserService,
            ISocialAccountService socialAccountService,
            INotificationManager notificationManager
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _socialUserService = socialUserService;
            _socialAccountService = socialAccountService;
            _twitterTreeWalker = new TweetTreeWalker();
            _notificationManager = notificationManager;
        }

        public async Task<TwitterProcessResult> ProcessTweet(SocialAccount currentAccount, ITweet currentTweet)
        {
            TwitterProcessResult result = new TwitterProcessResult(_notificationManager);
            IList<SocialAccount> allAccounts = _socialAccountService.FindAllTwitterAccounts().ToList();

            if (!ShouldProcess(currentAccount, allAccounts, currentTweet))
            {
                return new TwitterProcessResult(_notificationManager);
            }

            List<ITweet> tweets = _twitterTreeWalker.BuildTweetTree(currentTweet);
            return await AddTweets(currentAccount, allAccounts, tweets);
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

        private async Task<TwitterProcessResult> AddTweets(SocialAccount currentAccount, IList<SocialAccount> allAccounts, List<ITweet> tweets)
        {
            var result = new TwitterProcessResult(_notificationManager);
            if (!tweets.Any())
            {
                return result;
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
                    await AddTweet(result, currentAccount, allAccounts, tweets, tweet);
                    CurrentUnitOfWork.SaveChanges();
                });
            }

            return result;
        }

        private bool IsIntergrationAccountInvolved(ITweet tweet, IList<SocialAccount> allAccounts)
        {
            return IsIntegrationAccount(tweet.CreatedBy.IdStr, allAccounts) || tweet.UserMentions.Any(t => IsIntegrationAccount(t.IdStr, allAccounts));
        }

        private async Task AddTweet(TwitterProcessResult result, SocialAccount account, IList<SocialAccount> socialAccounts, List<ITweet> tweets, ITweet currentTweet)
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

                await SaveTweet(result, account, currentTweet, tweets, senderOriginalId, receiverOrignalId, customerOriginalIds);
            }

            // integration account sent to customer
            if (IsIntegrationAccount(senderOriginalId, socialAccounts)
                && !IsIntegrationAccount(receiverOrignalId, socialAccounts))
            {
                var customerOriginalIds = new List<string> { receiverOrignalId };
                await SaveTweet(result, account, currentTweet, tweets, senderOriginalId, receiverOrignalId, customerOriginalIds);
            }

            // customer sent to integration account
            if (!IsIntegrationAccount(senderOriginalId, socialAccounts)
                && IsIntegrationAccount(receiverOrignalId, socialAccounts))
            {
                var customerOriginalIds = new List<string> { senderOriginalId };
                await SaveTweet(result, account, currentTweet, tweets, senderOriginalId, receiverOrignalId, customerOriginalIds);
            }
        }

        private async Task SaveTweet(TwitterProcessResult result, SocialAccount account, ITweet currentTweet, List<ITweet> tweets, string senderOriginalId, string receiverOriginalId, List<string> customerOrginalIds)
        {
            var sender = await _socialUserService.GetOrCreateTwitterUser(senderOriginalId);
            var receiver = await _socialUserService.GetOrCreateTwitterUser(receiverOriginalId);
            var conversations = FindConversations(tweets, customerOrginalIds);
            var message = TwitterConverter.ConvertToMessage(currentTweet);
            message.SenderId = sender.Id;
            message.ReceiverId = receiver.Id;
            SaveConversations(result, account, conversations, message);
        }

        private IList<Conversation> FindConversations(IList<ITweet> tweets, List<string> customerOriginalIds)
        {
            List<string> tweetIds = tweets.Select(t => t.IdStr).ToList();
            return _conversationService.FindAll().Include(t => t.Messages)
                  .Where(c => c.Messages.Any(m => tweetIds.Contains(m.OriginalId)))
                  .Where(c => c.Messages.Any(m => customerOriginalIds.Contains(m.Sender.OriginalId) || customerOriginalIds.Contains(m.Receiver.OriginalId)))
                  .ToList();
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

        private void SaveConversations(TwitterProcessResult result, SocialAccount account, IList<Conversation> conversations, Message message)
        {
            if (conversations.Any())
            {
                foreach (var conversation in conversations)
                {
                    if (conversation.Messages.Any(t => t.OriginalId == message.OriginalId))
                    {
                        continue;
                    }
                    message.ConversationId = conversation.Id;
                    conversation.Messages.Add(message);
                    if (message.SendTime > conversation.LastMessageSentTime)
                    {
                        conversation.IfRead = false;
                        conversation.Status = message.SenderId == account.Id ? ConversationStatus.PendingExternal : ConversationStatus.PendingInternal;
                        conversation.LastMessageSenderId = message.SenderId;
                        conversation.LastMessageSentTime = message.SendTime;
                    }
                    _conversationService.Update(conversation);
                    CurrentUnitOfWork.SaveChanges();
                    result.WithUpdatedConversation(conversation);
                    result.WithNewMessage(message);
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
                    Subject = TwitterUtil.GetSubject(message.Content),
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };
                conversation.Messages.Add(message);
                _conversationService.AddConversation(account, conversation);
                CurrentUnitOfWork.SaveChanges();
                result.WithNewConversation(conversation);
            }
        }
    }
}
