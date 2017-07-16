using Framework.Core;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Application.AppServices
{
    public interface ITwitterAppService
    {
        Task ReceivedTweet(SocialAccount account, ITweet currentTweet);
    }

    public class TwitterAppService : AppService, ITwitterAppService
    {
        private ITwitterService _twitterService;

        public TwitterAppService(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }

        public async Task ReceivedTweet(SocialAccount account, ITweet currentTweet)
        {
            await _twitterService.ReceivedTweet(account, currentTweet);
        }
    }
}
