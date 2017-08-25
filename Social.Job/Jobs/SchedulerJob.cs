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

namespace Social.Job.Jobs
{
    public class SchedulerJob : JobBase, ITransient
    {
        private static RunningJobs RunningJobs = new RunningJobs();

        private IScheduleJobManager _scheduleJobManager;
        private IRepository<GeneralDataContext, SiteSocialAccount> _siteSocialAccountRepo;

        public SchedulerJob(
            IScheduleJobManager schedulerJobManager,
            IRepository<GeneralDataContext, SiteSocialAccount> siteSocialAccountRepo
            )
        {
            _scheduleJobManager = schedulerJobManager;
            _siteSocialAccountRepo = siteSocialAccountRepo;
        }

        public static void RemoveRunningJob<TJob>(int siteId, string originalAccountId) where TJob : JobBase
        {
            RunningJobs.Remove<TJob>(siteId, originalAccountId);
        }

        private async Task<List<SiteSocialAccount>> GetFacebookSiteAccounts()
        {
            List<SiteSocialAccount> accounts = new List<SiteSocialAccount>();

            await UnitOfWorkManager.RunWithoutTransaction(null, () =>
             {
                 return Task.Run(() =>
                      accounts = _siteSocialAccountRepo.FindAll().Where(t => t.FacebookPageId != null).ToList()
                     );
             });

            return accounts;
        }

        private async Task<List<SiteSocialAccount>> GetTwitterSiteAccounts()
        {
            List<SiteSocialAccount> accounts = new List<SiteSocialAccount>();

            await UnitOfWorkManager.RunWithoutTransaction(null, () =>
                {
                    return Task.Run(() =>
                     accounts = _siteSocialAccountRepo.FindAll().Where(t => t.TwitterUserId != null).ToList()
                    );
                });

            return accounts;
        }

        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            List<SiteSocialAccount> facebookAccounts = await GetFacebookSiteAccounts();
            List<SiteSocialAccount> twitterAccounts = await GetTwitterSiteAccounts();

            foreach (var facebookAccount in facebookAccounts)
            {
                RunningJobs.Schedule<PullTaggedVisitorPostsJob>(_scheduleJobManager, facebookAccount, CronTrigger(AppSettings.FacebookPullTaggedVisitorPostsJobCronExpression));
                RunningJobs.Schedule<PullVisitorPostsFromFeedJob>(_scheduleJobManager, facebookAccount, CronTrigger(AppSettings.FacebookPullVisitorPostsFromFeedJobCronExpression));
                RunningJobs.Schedule<PullMessagesJob>(_scheduleJobManager, facebookAccount, CronTrigger(AppSettings.FacebookPullMessagesJobCronExpression));
            }

            foreach (var twitterAccount in twitterAccounts)
            {
                RunningJobs.Schedule<TwitterUserStreamJob>(_scheduleJobManager, twitterAccount, StartNowTrigger());
                RunningJobs.Schedule<TwitterPullDirectMessagesJob>(_scheduleJobManager, twitterAccount, CronTrigger(AppSettings.TwitterPullDirectMessagesJobCronExpression));
                RunningJobs.Schedule<TwitterPullTweetsJob>(_scheduleJobManager, twitterAccount, CronTrigger(AppSettings.TwitterPullTweetsJobCronExpression));
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
