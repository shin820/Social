using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl.Matchers;

namespace Social.Job.Jobs
{
    public class SchedulerJob : JobBase, ITransient
    {
        private IScheduleJobManager _scheduleJobManager;

        public SchedulerJob(IScheduleJobManager schedulerJobManager)
        {
            _scheduleJobManager = schedulerJobManager;
        }

        protected override Task ExecuteJob(IJobExecutionContext context)
        {
            TaskCompletionSource<object> taskSrc = new TaskCompletionSource<object>();

            int[] siteIds = new int[] { 10000 };
            ScheduleFacebookWebHookJob(siteIds, context);
            taskSrc.SetResult(null);
            return taskSrc.Task;
        }

        private void ScheduleFacebookWebHookJob(int[] siteIds, IJobExecutionContext context)
        {
            const string groupName = "FacebookWebHookJobGroup";

            foreach (var siteId in siteIds)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupEquals(groupName);
                var jobKeys = context.Scheduler.GetJobKeys(groupMatcher);
                string jobKey = GetFacebookWebHookJobKey(siteId);
                if (jobKeys.All(t => t.Name != jobKey))
                {
                    _scheduleJobManager.ScheduleAsync<FacebookWebHookJob, int>(
                    job =>
                    {
                        job.WithIdentity(jobKey, groupName);
                    },
                    trigger =>
                    {
                        trigger
                        //.WithCronSchedule("0 0/5 * * * ?", x => x.WithMisfireHandlingInstructionDoNothing())
                        .WithCalendarIntervalSchedule(x => x.WithIntervalInMinutes(5))
                        .Build();
                    },
                    siteId
                    );
                }
            }
        }

        private string GetFacebookWebHookJobKey(int siteId)
        {
            return $"FacebookWebHookJobKey - {siteId}";
        }
    }
}
