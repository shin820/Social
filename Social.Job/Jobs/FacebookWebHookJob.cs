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

namespace Social.Job.Jobs
{
    public class FacebookWebHookJob : JobBase, ITransient
    {
        private IFacebookAppService _fbWebHookAppService;
        private IRepository<FacebookWebHookRawData> _hookRawDataRepo;
        public FacebookWebHookJob(
            IFacebookAppService fbWebHookAppService,
            IRepository<FacebookWebHookRawData> hookRawDataRepo
            )
        {
            _fbWebHookAppService = fbWebHookAppService;
            _hookRawDataRepo = hookRawDataRepo;
        }


        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            int siteId = context.JobDetail.GetCustomData<int>();
            if (siteId == 0)
            {
                return;
            }

            List<FacebookWebHookRawData> rawDataList = new List<FacebookWebHookRawData>();
            using (var uow = UnitOfWorkManager.Begin())
            {
                using (CurrentUnitOfWork.SetSiteId(siteId))
                {
                    rawDataList = _hookRawDataRepo.FindAll().Where(t => t.IsDeleted == false).OrderBy(t => t.CreatedTime).Take(50).ToList();
                    uow.Complete();
                }
            }

            foreach (FacebookWebHookRawData rawData in rawDataList)
            {
                using (var uow = UnitOfWorkManager.Begin())
                {
                    using (CurrentUnitOfWork.SetSiteId(siteId))
                    {
                        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<FbHookData>(rawData.Data);
                        await _fbWebHookAppService.ProcessWebHookData(data);
                        _hookRawDataRepo.Delete(rawData);
                        uow.Complete();
                    }
                }
            }
        }
    }
}
