using System;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Quartz;
using Framework.Core;
using Framework.Core.UnitOfWork;

namespace Social.Job
{
    public abstract class JobBase : IJob
    {
        protected static ILog Logger = SchedulerLogger.GetLogger();

        private IUnitOfWorkManager _unitOfWorkManager;
        public IUnitOfWorkManager UnitOfWorkManager
        {
            get
            {
                if (_unitOfWorkManager == null)
                {
                    throw new InvalidOperationException("Must set UnitOfWorkManager before use it.");
                }

                return _unitOfWorkManager;
            }
            set { _unitOfWorkManager = value; }
        }

        protected IUnitOfWork CurrentUnitOfWork
        {
            get { return UnitOfWorkManager.Current; }
        }

        public async void Execute(IJobExecutionContext context)
        {

            JobKey jobKey = context.JobDetail.Key;
            Logger.Info($"{jobKey} start executing.");

            try
            {
                await ExecuteJob(context);
            }
            catch (Exception ex)
            {
                Logger.Error($"{jobKey} executed failed.", ex);
            }

            Logger.Info($"{jobKey} executed complete.");
        }

        protected abstract Task ExecuteJob(IJobExecutionContext context);
    }
}
