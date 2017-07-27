using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Social.Application.AppServices;

namespace Social.Job.Jobs.Facebook
{
    public class PullMassagesJob : JobBase, ITransient
    {
        private IFacebookAppService _sevice;

        public PullMassagesJob(IFacebookAppService service)
        {
            _sevice = service;
        }
        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            var socialAccount = await GetFacebookSocialAccount(context);
            if(socialAccount == null)
            {
                return;
            }

            await UnitOfWorkManager.RunWithoutTransaction(socialAccount.SiteId, async () =>
           {
               await _sevice.PullMassagesJob(socialAccount);
           });
        }
    }
}
