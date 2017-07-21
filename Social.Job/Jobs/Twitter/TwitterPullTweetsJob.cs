using Framework.Core;
using Quartz;
using Social.Application.AppServices;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Job.Jobs
{
    public class TwitterPullTweetsJob : JobBase, ITransient
    {
        private ITwitterAppService _twitterAppService;

        public TwitterPullTweetsJob(
            ITwitterAppService twitterAppService
            )
        {
            _twitterAppService = twitterAppService;
        }

        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            SocialAccount socialAccount = await GetTwitterSocialAccount(context);
            if (socialAccount == null)
            {
                return;
            }

            await _twitterAppService.PullTweets(socialAccount);
        }
    }
}
