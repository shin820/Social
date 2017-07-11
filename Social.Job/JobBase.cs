using System;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Quartz;
using Framework.Core;

namespace Social.Job
{
    public abstract class JobBase : IJob
    {
        protected static DependencyResolver DependencyResolver;
        protected static ILog Logger = SchedulerLogger.GetLogger();

        static JobBase()
        {
            DependencyResolver = new DependencyResolver();
            DependencyResolver.Install(new JobInstaller());
        }

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
