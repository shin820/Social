using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Framework.Core;
using Social.Application.AppServices;

namespace Social.Job.Jobs.Facebook
{
    public class GetRawDataJob : JobBase, ITransient
    {
        private IMessageAttachmentAppService _sevice;

        public GetRawDataJob(IMessageAttachmentAppService service)
        {
            _sevice = service;
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
                await _sevice.GetRawDataJob();
            });
        }
    }
}
