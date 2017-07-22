using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Social.Domain.DomainServices;
using Framework.Core.UnitOfWork;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System.Data.Entity;
using Social.Application.AppServices;

namespace Social.Job.Jobs.Facebook
{
    public class PullVisitorPostsFromFeedJob : JobBase, ITransient
    {
        private IFacebookAppService _service;

        public PullVisitorPostsFromFeedJob(
            IFacebookAppService service
            )
        {
            _service = service;
        }


        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            var socialAccount = await GetFacebookSocialAccount(context);
            if (socialAccount == null)
            {
                return;
            }

            await UnitOfWorkManager.RunWithoutTransaction(socialAccount.SiteId, async () =>
            {
                await _service.PullVisitorPostsFromFeed(socialAccount);
            });
        }
    }
}
