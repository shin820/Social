﻿using Framework.Core;
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

namespace Social.Job.Jobs
{
    public class FacebookTaggedVisitorPostJob : JobBase, ITransient
    {
        private ITaggedVisitorPostService _service;
        private IRepository<SocialAccount> _socialAccountRepo;

        public FacebookTaggedVisitorPostJob(
            ITaggedVisitorPostService service,
            IRepository<SocialAccount> socialAccountRepo
            )
        {
            _service = service;
            _socialAccountRepo = socialAccountRepo;
        }


        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            int siteId = context.JobDetail.GetCustomData<int>();
            if (siteId == 0)
            {
                return;
            }

            using (var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = false }))
            {
                SocialAccount account = _socialAccountRepo.FindAll().Include(t => t.SocialUser).FirstOrDefault(t => t.SiteId == siteId && t.SocialUser.Type == SocialUserType.Facebook && t.IfEnable);

                if (account != null)
                {
                    await _service.Process(account);
                }

                uow.Complete();
            }
        }
    }
}
