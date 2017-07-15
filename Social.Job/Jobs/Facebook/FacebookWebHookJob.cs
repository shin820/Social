using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Framework.Core;
using Social.Domain.Entities;
using Framework.Core.UnitOfWork;
using Social.Infrastructure.Facebook;
using Social.Application.AppServices;
using Social.Infrastructure.Enum;
using System.Data.Entity;

namespace Social.Job.Jobs
{
    public class FacebookWebHookJob : JobBase, ITransient
    {
        private IFacebookAppService _fbWebHookAppService;
        private IRepository<FacebookWebHookRawData> _hookRawDataRepo;
        private IRepository<SocialAccount> _socialAccountRepo;

        public FacebookWebHookJob(
            IFacebookAppService fbWebHookAppService,
            IRepository<FacebookWebHookRawData> hookRawDataRepo,
            IRepository<SocialAccount> socialAccountRepo
            )
        {
            _fbWebHookAppService = fbWebHookAppService;
            _hookRawDataRepo = hookRawDataRepo;
            _socialAccountRepo = socialAccountRepo;
        }


        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            int siteId = context.JobDetail.GetCustomData<int>();
            if (siteId == 0)
            {
                return;
            }

            List<FacebookWebHookRawData> rawDataList = new List<FacebookWebHookRawData>();
            using (var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = false }))
            {
                using (CurrentUnitOfWork.SetSiteId(siteId))
                {
                    SocialAccount account = _socialAccountRepo.FindAll().Include(t => t.SocialUser).FirstOrDefault(t => t.SocialUser.Type == SocialUserType.Facebook && t.IfEnable);
                    if (account != null)
                    {
                        rawDataList = _hookRawDataRepo.FindAll().Where(t => t.IsDeleted == false).OrderBy(t => t.CreatedTime).Take(50).ToList();

                        foreach (FacebookWebHookRawData rawData in rawDataList)
                        {
                            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<FbHookData>(rawData.Data);
                            await _fbWebHookAppService.ProcessWebHookData(account, data);
                            _hookRawDataRepo.Delete(rawData);
                        }
                    }
                    uow.Complete();
                }
            }
        }
    }
}
