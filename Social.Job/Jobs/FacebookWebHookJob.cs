using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

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
                 Console.WriteLine("11111");
             });
        }
    }
}
