using System;
using System.Threading.Tasks;
using Quartz;

namespace Social.Job
{
    public interface IScheduleJobManager
    {
        Task ScheduleAsync<TJob>(Action<JobBuilder> configureJob, Action<TriggerBuilder> configureTrigger) where TJob : IJob;
        void Shutdown();
        void Start();
    }
}