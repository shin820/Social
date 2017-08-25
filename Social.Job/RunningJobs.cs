using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Social.Domain.Entities;
using Quartz;

namespace Social.Job
{
    public class RunningJobs
    {
        private ConcurrentDictionary<string, RunningJob> _runningJobs = new ConcurrentDictionary<string, RunningJob>();

        public bool IsRunning<TJob>(int siteId, string originalAccountId) where TJob : JobBase
        {
            string key = GetJobKey<TJob>(siteId, originalAccountId);
            return _runningJobs.ContainsKey(key);
        }

        public void Schedule<TJob>(IScheduleJobManager scheduleJobManager, SiteSocialAccount account, Action<TriggerBuilder> configureTrigger) where TJob : JobBase
        {
            string originalAccountId = !string.IsNullOrWhiteSpace(account.FacebookPageId) ? account.FacebookPageId : account.TwitterUserId;
            if (string.IsNullOrWhiteSpace(originalAccountId))
            {
                return;
            }

            string groupName = GetJobGroup<TJob>();
            string jobKey = GetJobKey<TJob>(account.SiteId, originalAccountId);
            bool isRunning = IsRunning<TJob>(account.SiteId, originalAccountId);
            if (!isRunning)
            {
                scheduleJobManager.ScheduleAsync<TJob, SiteSocialAccount>(
                job =>
                {
                    job.WithIdentity(jobKey, groupName);
                },
                configureTrigger,
                account
                );
            }

            var runningJob = new RunningJob
            {
                JobKey = jobKey,
                JobGroup = groupName,
                SiteId = account.SiteId,
                OriginalAccountId = originalAccountId,
                LastScheduleTime = DateTime.UtcNow
            };

            _runningJobs.AddOrUpdate(jobKey, runningJob, (k, job) => { return runningJob; });
        }

        public void StopTimeoutJobs(IScheduler scheduler)
        {
            var timemOutJobs = _runningJobs.Where(t => t.Value.IsTimeout).ToList();
            foreach (var timemOutJob in timemOutJobs)
            {
                JobKey jobKey = new JobKey(timemOutJob.Key, timemOutJob.Value.JobGroup);
                IJobDetail jobDetail = scheduler.GetJobDetail(jobKey);
                if (jobDetail != null)
                {
                    bool isDeleted = scheduler.DeleteJob(jobKey);
                }

                RunningJob stopjob;
                _runningJobs.TryRemove(timemOutJob.Key, out stopjob);
            }
        }

        public static string GetJobKey<TJob>(int siteId, string originalAccountId) where TJob : JobBase
        {
            return $"{typeof(TJob).Name} - SiteId({siteId}) - OriginalId({originalAccountId})";
        }

        public static string GetJobGroup<TJob>() where TJob : JobBase
        {
            return typeof(TJob).Name;
        }

        public void Remove<TJob>(int siteId, string originalAccountId) where TJob : JobBase
        {
            string jobKey = GetJobKey<TJob>(siteId, originalAccountId);
            RunningJob removedJob;
            _runningJobs.TryRemove(jobKey, out removedJob);
        }

        private class RunningJob
        {
            public string JobKey { get; set; }
            public string JobGroup { get; set; }
            public int SiteId { get; set; }
            public string OriginalAccountId { get; set; }
            public DateTime LastScheduleTime { get; set; }

            public bool IsTimeout
            {
                get
                {
                    return (DateTime.UtcNow - LastScheduleTime).TotalSeconds > 300;
                }
            }
        }
    }
}
