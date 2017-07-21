using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Parameters;

namespace Social.Domain.DomainServices
{
    public interface ITwitterPullJobService
    {
        Task PullDirectMessages(SocialAccount account);
        Task PullTweets(SocialAccount account);
    }

    public class TwitterPullJobService : ServiceBase, ITwitterPullJobService
    {
        private ITwitterService _twitterService;

        public TwitterPullJobService(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }

        #region PullDirectMessages
        public async Task PullDirectMessages(SocialAccount account)
        {
            int maxNumberOfMessagesRetrieve = 100;
            DateTime since = DateTime.UtcNow.AddDays(-1);
            Auth.SetUserCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, account.Token, account.TokenSecret);
            await PullReceivedDirectMessages(account, maxNumberOfMessagesRetrieve, since);
            await PullSentDirectMessages(account, maxNumberOfMessagesRetrieve, since);
        }

        private async Task PullReceivedDirectMessages(SocialAccount account, int maxNumberOfMessagesRetrieve, DateTime since)
        {
            var receivedDirectMessages = Tweetinvi.Message.GetLatestMessagesReceived(maxNumberOfMessagesRetrieve);
            while (receivedDirectMessages.Any())
            {
                if (receivedDirectMessages.First().CreatedAt <= since)
                {
                    break;
                }

                foreach (var message in receivedDirectMessages)
                {
                    if (message.CreatedAt <= since)
                    {
                        break;
                    }
                    await UnitOfWorkManager.RunWithNewTransaction(account.SiteId, async () =>
                    {
                        await _twitterService.ProcessDirectMessage(account, message);
                    });
                };

                if (receivedDirectMessages.Any(t => t.CreatedAt <= since))
                {
                    break;
                }

                var maxId = receivedDirectMessages.Last().Id;
                var parameter = new MessagesReceivedParameters { MaxId = maxId, MaximumNumberOfMessagesToRetrieve = maxNumberOfMessagesRetrieve };
                receivedDirectMessages = Tweetinvi.Message.GetLatestMessagesReceived(parameter);
            }
        }

        private async Task PullSentDirectMessages(SocialAccount account, int maxNumberOfMessagesRetrieve, DateTime since)
        {
            var sentDirectMessages = Tweetinvi.Message.GetLatestMessagesSent(maxNumberOfMessagesRetrieve);
            while (sentDirectMessages.Any())
            {
                if (sentDirectMessages.First().CreatedAt <= since)
                {
                    break;
                }

                foreach (var message in sentDirectMessages)
                {
                    if (message.CreatedAt <= since)
                    {
                        break;
                    }
                    await UnitOfWorkManager.RunWithNewTransaction(account.SiteId, async () =>
                    {
                        await _twitterService.ProcessDirectMessage(account, message);
                    });
                };

                if (sentDirectMessages.Any(t => t.CreatedAt <= since))
                {
                    break;
                }

                var maxId = sentDirectMessages.Last().Id;
                var parameter = new MessagesSentParameters { MaxId = maxId, MaximumNumberOfMessagesToRetrieve = maxNumberOfMessagesRetrieve };
                sentDirectMessages = Tweetinvi.Message.GetLatestMessagesSent(parameter);
            }
        }
        #endregion

        #region PullTweets
        public async Task PullTweets(SocialAccount account)
        {
            int maxNumberOfTweetsRetrieve = 40;
            DateTime since = DateTime.UtcNow.AddDays(-1);
            Auth.SetUserCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, account.Token, account.TokenSecret);
            await PullUserTimeLineTweets(account, maxNumberOfTweetsRetrieve, since);
        }

        private async Task PullUserTimeLineTweets(SocialAccount account, int maxNumberOfTweetsRetrieve, DateTime since)
        {
            var tweets = Timeline.GetUserTimeline(long.Parse(account.SocialUser.OriginalId), maxNumberOfTweetsRetrieve);
            while (tweets.Any())
            {
                if (tweets.First().CreatedAt <= since)
                {
                    break;
                }

                foreach (var tweet in tweets)
                {
                    if (tweet.CreatedAt <= since)
                    {
                        break;
                    }
                    await UnitOfWorkManager.RunWithNewTransaction(account.SiteId, async () =>
                    {
                        await _twitterService.ProcessTweet(account, tweet);
                    });
                };

                if (tweets.Any(t => t.CreatedAt <= since))
                {
                    break;
                }

                var maxId = tweets.Last().Id;
                var parameter = new UserTimelineParameters { MaxId = maxId, MaximumNumberOfTweetsToRetrieve = maxNumberOfTweetsRetrieve };
                tweets = Timeline.GetUserTimeline(long.Parse(account.SocialUser.OriginalId), parameter);
            }
        }
        private async Task PullMentionTimeLineTweets(SocialAccount account, int maxNumberOfTweetsRetrieve, DateTime since)
        {
            var tweets = Timeline.GetMentionsTimeline(maxNumberOfTweetsRetrieve);
            while (tweets.Any())
            {
                if (tweets.First().CreatedAt <= since)
                {
                    break;
                }

                foreach (var tweet in tweets)
                {
                    if (tweet.CreatedAt <= since)
                    {
                        break;
                    }
                    await UnitOfWorkManager.RunWithNewTransaction(account.SiteId, async () =>
                    {
                        await _twitterService.ProcessTweet(account, tweet);
                    });
                };

                if (tweets.Any(t => t.CreatedAt <= since))
                {
                    break;
                }

                var maxId = tweets.Last().Id;
                var parameter = new MentionsTimelineParameters { MaxId = maxId, MaximumNumberOfTweetsToRetrieve = maxNumberOfTweetsRetrieve };
                tweets = Timeline.GetMentionsTimeline(parameter);
            }
        }
        #endregion

    }
}
