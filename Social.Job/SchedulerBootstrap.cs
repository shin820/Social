using Common.Logging;
using Quartz;
using Quartz.Impl;
using Social.Job.Jobs;
using System;
using System.Net;

namespace Social.Job
{
    public class SchedulerBootstrap
    {
        private readonly IScheduler _scheduler;
        private readonly ILog _log = SchedulerLogger.GetLogger();

        public SchedulerBootstrap()
        {
            _log.Info("------- Initializing ----------------------");
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler();
            _log.Info("------- Initialization Complete -----------");

            _log.Info("------- Scheduling Job  -------------------");

            try
            {
                new FacebookWebHookJob().Register(_scheduler);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void Start()
        {
            _scheduler.Start();
            _log.Info("------- Started Scheduler -----------------");
        }

        public void Stop()
        {
            _log.Info("------- Shutting Down ---------------------");
            _scheduler.Shutdown(true);
            _log.Info("------- Shutdown Complete -----------------");
        }
    }
}
