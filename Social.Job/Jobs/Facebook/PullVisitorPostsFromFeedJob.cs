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
        private ISocialAccountService _socialAccountService;

        public PullVisitorPostsFromFeedJob(
            IFacebookAppService service,
            ISocialAccountService socialAccountService
            )
        {
            _service = service;
            _socialAccountService = socialAccountService;
        }


        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            var siteSocicalAccount = context.JobDetail.GetCustomData<SiteSocialAccount>();
            if (siteSocicalAccount == null)
            {
                return;
            }

            int siteId = siteSocicalAccount.SiteId;
            string facebookPageId = siteSocicalAccount.FacebookPageId;

            await UnitOfWorkManager.RunWithoutTransaction(siteId, async () =>
            {
                SocialAccount account = await _socialAccountService.GetAccountAsync(SocialUserType.Facebook, facebookPageId);
                if (account != null)
                {
                    await _service.PullVisitorPostsFromFeed(account);
                }
            });
        }
    }
}
