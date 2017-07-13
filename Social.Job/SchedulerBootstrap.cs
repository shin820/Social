using Common.Logging;
using Framework.Core;
using Quartz;
using Quartz.Impl;
using Social.Job.Jobs;
using System;
using System.Net;

namespace Social.Job
{
    public class SchedulerBootstrap
    {
        private IScheduleJobManager _scheduleJobManager;

        public SchedulerBootstrap()
        {
            var dependencyResolver = new DependencyResolver();
            dependencyResolver.Install(new JobInstaller());

            _scheduleJobManager = dependencyResolver.Resolve<IScheduleJobManager>();

            _scheduleJobManager.ScheduleAsync<SchedulerJob>(
            job =>
            {
                job.WithDescription("ScheduleJob").WithIdentity("ScheduleJobKey");
            },
            trigger =>
            {
                trigger.WithIdentity("ScheduleJobJobTrigger")
                .WithDescription("ScheduleJobTriggerDescription")
                //.WithCronSchedule("0 0/1 * * * ?", x => x.WithMisfireHandlingInstructionDoNothing())
                .WithCalendarIntervalSchedule(x => x.WithIntervalInSeconds(10))
                .StartNow()
                .Build();
            });
        }

        public void Start()
        {
            _scheduleJobManager.Start();
        }

        public void Stop()
        {
            _scheduleJobManager.Shutdown();
        }
    }
}
