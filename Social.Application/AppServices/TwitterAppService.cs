using Framework.Core;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Social.Application.AppServices
{
    public interface ITwitterAppService
    {
        Task ProcessTweet(SocialAccount account, ITweet currentTweet);
        Task ProcessDirectMessage(SocialAccount account, IMessage directMsg);
        Task PullDirectMessages(SocialAccount account);
        Task PullTweets(SocialAccount account);
    }

    public class TwitterAppService : AppService, ITwitterAppService
    {
        private ITwitterService _twitterService;
        private ITwitterPullJobService _twitterPullJobService;

        public TwitterAppService(
            ITwitterService twitterService,
            ITwitterPullJobService twitterPullJobService
            )
        {
            _twitterService = twitterService;
            _twitterPullJobService = twitterPullJobService;
        }

        public async Task PullDirectMessages(SocialAccount account)
        {
            await _twitterPullJobService.PullDirectMessages(account);
        }

        public async Task PullTweets(SocialAccount account)
        {
            await _twitterPullJobService.PullTweets(account);
        }

        public async Task ProcessTweet(SocialAccount account, ITweet currentTweet)
        {
            await UnitOfWorkManager.RunWithoutTransaction(account.SiteId, async () =>
            {
                await _twitterService.ProcessTweet(account, currentTweet);
            });
        }

        public async Task ProcessDirectMessage(SocialAccount account, IMessage directMsg)
        {
            await UnitOfWorkManager.RunWithNewTransaction(account.SiteId, async () =>
            {
                await _twitterService.ProcessDirectMessage(account, directMsg);
            });
        }
    }
}
