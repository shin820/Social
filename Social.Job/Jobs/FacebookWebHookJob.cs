using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Framework.Core;
using Social.Domain.Entities;
using Framework.EntityFramework.UnitOfWork;

namespace Social.Job.Jobs
{
    public class FacebookWebHookJob : JobBase
    {
        public override void Register(IScheduler scheduler)
        {
            IJobDetail job = JobBuilder.Create(this.GetType())
              .WithIdentity(this.GetType().FullName)
              .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(this.GetType().FullName)
                //.WithCronSchedule("0 0/5 * * * ?", x => x.WithMisfireHandlingInstructionDoNothing())
                .StartNow()
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }

        protected override Task ExecuteJob(IJobExecutionContext context)
        {
            return Task.Run(() =>
             {
                 var uowManager = DependencyResolver.Resolve<IUnitOfWorkManager>();
                 using (var uow = uowManager.Begin())
                 {
                     var repo = DependencyResolver.Resolve<IRepository<Conversation>>();
                     var a = repo.FindAll().ToList();
                     uow.Complete();
                 }
             });
        }
    }
}
