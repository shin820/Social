using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Social.Job.Jobs.Facebook;
using Social.Domain;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Domain.Core;
using Social.Domain.DomainServices;

namespace Social.Job.Jobs
{
    public class SchedulerJob : JobBase, ITransient
    {
        private static RunningJobs RunningJobs = new RunningJobs();

        private IScheduleJobManager _scheduleJobManager;
        private ISiteSocialAccountService _siteSocialAccountService;

        public SchedulerJob(
            IScheduleJobManager schedulerJobManager,
            ISiteSocialAccountService siteSocialAccountService
            )
        {
            _scheduleJobManager = schedulerJobManager;
            _siteSocialAccountService = siteSocialAccountService;
        }

        public static void RemoveRunningJob<TJob>(int siteId, string originalAccountId) where TJob : JobBase
        {
            RunningJobs.Remove<TJob>(siteId, originalAccountId);
        }

        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            List<SiteSocialAccount> facebookAccounts = await _siteSocialAccountService.GetFacebookSiteAccountsAsync();
            List<SiteSocialAccount> twitterAccounts = await _siteSocialAccountService.GetTwitterSiteAccountsAsync();

            if (facebookAccounts != null && facebookAccounts.Any())
            {
                foreach (var facebookAccount in facebookAccounts)
                {
                    RunningJobs.Schedule<PullTaggedVisitorPostsJob>(_scheduleJobManager, facebookAccount, CronTrigger(AppSettings.FacebookPullTaggedVisitorPostsJobCronExpression));
                    RunningJobs.Schedule<PullVisitorPostsFromFeedJob>(_scheduleJobManager, facebookAccount, CronTrigger(AppSettings.FacebookPullVisitorPostsFromFeedJobCronExpression));
                    RunningJobs.Schedule<PullMessagesJob>(_scheduleJobManager, facebookAccount, CronTrigger(AppSettings.FacebookPullMessagesJobCronExpression));
                }
            }

            if (twitterAccounts != null && twitterAccounts.Any())
            {
                foreach (var twitterAccount in twitterAccounts)
                {
                    RunningJobs.Schedule<TwitterUserStreamJob>(_scheduleJobManager, twitterAccount, StartNowTrigger());
                    RunningJobs.Schedule<TwitterPullDirectMessagesJob>(_scheduleJobManager, twitterAccount, CronTrigger(AppSettings.TwitterPullDirectMessagesJobCronExpression));
                    RunningJobs.Schedule<TwitterPullTweetsJob>(_scheduleJobManager, twitterAccount, CronTrigger(AppSettings.TwitterPullTweetsJobCronExpression));
                }
            }

            RunningJobs.StopTimeoutJobs(context.Scheduler);
        }

        private Action<TriggerBuilder> CronTrigger(string cronExpression)
        {
            return trigger =>
            {
                trigger.WithCronSchedule(cronExpression, x => x.WithMisfireHandlingInstructionIgnoreMisfires()).Build();
            };
        }

        private Action<TriggerBuilder> StartNowTrigger()
        {
            return trigger =>
            {
                trigger.StartNow().Build();
            };
        }
    }
}
