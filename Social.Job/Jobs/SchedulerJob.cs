using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl.Matchers;
using Social.Job.Jobs.Facebook;
using Social.Domain;
using Social.Domain.Entities;
using Social.Infrastructure;
using System.Collections.Concurrent;

namespace Social.Job.Jobs
{
    public class SchedulerJob : JobBase, ITransient
    {
        private static ConcurrentBag<string> RegistedJobs = new ConcurrentBag<string>();

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
            TaskCompletionSource<object> taskSrc = new TaskCompletionSource<object>();

            List<SiteSocialAccount> facebookAccounts = await GetFacebookSiteAccounts();
            List<SiteSocialAccount> twitterAccounts = await GetTwitterSiteAccounts();

            foreach (var facebookAccount in facebookAccounts)
            {
                //ScheduleJob<FacebookWebHookJob>(facebookAccount, context);
                //ScheduleJob<PullTaggedVisitorPostsJob>(facebookAccount, context, CronTrigger(AppSettings.FacebookPullTaggedVisitorPostsJobCronExpression));
                //ScheduleJob<PullVisitorPostsFromFeedJob>(facebookAccount, context, CronTrigger(AppSettings.FacebookPullVisitorPostsFromFeedJobCronExpression));
                //ScheduleJob<PullMassagesJob>(facebookAccount, context, CronTrigger(AppSettings.FacebookPullVisitorPostsFromFeedJobCronExpression));
            }

            foreach (var twitterAccount in twitterAccounts)
            {
                //ScheduleJob<TwitterUserStreamJob>(twitterAccount, context, StartNowTrigger());
                //ScheduleJob<TwitterPullDirectMessagesJob>(twitterAccount, context, CronTrigger(AppSettings.TwitterPullDirectMessagesJobCronExpression));
                ScheduleJob<TwitterPullTweetsJob>(twitterAccount, context, StartNowTrigger());
            }
        }

        private void ScheduleJob<TJob>(SiteSocialAccount account, IJobExecutionContext context, Action<TriggerBuilder> configureTrigger) where TJob : JobBase
        {
            string originalId = !string.IsNullOrWhiteSpace(account.FacebookPageId) ? account.FacebookPageId : account.TwitterUserId;
            if (string.IsNullOrWhiteSpace(originalId))
            {
                return;
            }
            string groupName = GetJobGroup<TJob>();
            var currentExecutingJobs = context.Scheduler.GetCurrentlyExecutingJobs();
            string jobKey = GetJobKey<TJob>(account.SiteId, originalId);
            bool hasSameJobRunning = RegistedJobs.Contains(jobKey);
            if (!hasSameJobRunning)
            {
                _scheduleJobManager.ScheduleAsync<TJob, SiteSocialAccount>(
                job =>
                {
                    job.WithIdentity(jobKey, groupName);
                },
                configureTrigger,
                account
                );
                RegistedJobs.Add(jobKey);
            }
        }

        private Action<TriggerBuilder> CronTrigger(string cronExpression)
        {
            return trigger =>
            {
                trigger.WithCronSchedule(cronExpression).Build();
            };
        }

        private Action<TriggerBuilder> StartNowTrigger()
        {
            return trigger =>
            {
                trigger.StartNow().Build();
            };
        }

        private string GetJobGroup<TJob>() where TJob : JobBase
        {
            return typeof(TJob).Name;
        }

        private string GetJobKey<TJob>(int siteId, string originalId) where TJob : JobBase
        {
            return $"{typeof(TJob).Name} - SiteId({siteId}) - OriginalId({originalId})";
        }
    }
}
