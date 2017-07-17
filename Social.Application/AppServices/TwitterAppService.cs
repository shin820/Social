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

namespace Social.Application.AppServices
{
    public interface ITwitterAppService
    {
        Task ProcessTweet(SocialAccount account, ITweet currentTweet);
        Task ProcessDirectMessage(SocialAccount account, IMessage directMsg);
    }

    public class TwitterAppService : AppService, ITwitterAppService
    {
        private ITwitterService _twitterService;

        public TwitterAppService(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }

        public async Task ProcessTweet(SocialAccount account, ITweet currentTweet)
        {
            await UnitOfWorkManager.RunWithNewTransaction(account.SiteId, async () =>
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
