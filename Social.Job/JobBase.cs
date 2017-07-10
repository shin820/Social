using System;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Quartz;

namespace Social.Job
{
    public abstract class JobBase : IJob
    {
        protected static ILog Logger = SchedulerLogger.GetLogger();

        public async void Execute(IJobExecutionContext context)
        {

            JobKey jobKey = context.JobDetail.Key;
            Logger.InfoFormat("{0} start executing.", jobKey);

            try
            {
                await ExecuteJob(context);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("{0} executed failed.", ex, jobKey);
            }

            Logger.InfoFormat("{0} executed complete.", jobKey);
        }

        protected abstract Task ExecuteJob(IJobExecutionContext context);

        public abstract void Register(IScheduler scheduler);
    }
}
