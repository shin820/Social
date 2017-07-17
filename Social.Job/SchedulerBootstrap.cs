using Common.Logging;
using Framework.Core;
using Quartz;
using Quartz.Impl;
using Social.Application;
using Social.Domain;
using Social.Job.Jobs;
using System;
using System.Net;
using System.Reflection;

namespace Social.Job
{
    public class SchedulerBootstrap
    {
        private IScheduleJobManager _scheduleJobManager;

        public SchedulerBootstrap()
        {
            var dependencyResolver = new DependencyResolver();
            dependencyResolver.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
            new ApplicationServicesRegistrar(dependencyResolver).RegisterServices();

            _scheduleJobManager = dependencyResolver.Resolve<IScheduleJobManager>();

            //_scheduleJobManager.ScheduleAsync<SchedulerJob>(
            //job =>
            //{
            //    job.WithIdentity("ScheduleJobKey");
            //},
            //trigger =>
            //{
            //    trigger/*.WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever().WithMisfireHandlingInstructionIgnoreMisfires())*/
            //    .StartNow()
            //    .Build();
            //});

            _scheduleJobManager.ScheduleAsync<TwitterStreamJob, int>(
            job =>
            {
                job.WithIdentity("TwitterStreamJobKey");
            },
            trigger =>
            {
                trigger/*.WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever().WithMisfireHandlingInstructionIgnoreMisfires())*/
                .StartNow()
                .Build();
            }, 10000);
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
