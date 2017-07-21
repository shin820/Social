using Framework.Core;
using Social.Application.AppServices;
using Social.Domain.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Social.Domain.Entities;

namespace Social.Job.Jobs
{
    public class TwitterPullDirectMessagesJob : JobBase, ITransient
    {
        private ITwitterAppService _twitterAppService;

        public TwitterPullDirectMessagesJob(
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

            await _twitterAppService.PullDirectMessages(socialAccount);
        }
    }
}
