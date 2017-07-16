using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Tweetinvi.Models;
using Tweetinvi;
using Social.Application.AppServices;
using Framework.Core.UnitOfWork;
using Social.Domain.Entities;
using System.Data.Entity;
using Social.Infrastructure.Enum;

namespace Social.Job.Jobs
{
    public class TwitterStreamJob : JobBase, ITransient
    {
        private ITwitterAppService _twitterAppService;
        private IRepository<SocialAccount> _socialAccountRepo;

        public TwitterStreamJob(
            ITwitterAppService twitterAppService,
            IRepository<SocialAccount> socialAccountRepo
            )
        {
            _twitterAppService = twitterAppService;
            _socialAccountRepo = socialAccountRepo;
        }

        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            int siteId = context.JobDetail.GetCustomData<int>();
            if (siteId == 0)
            {
                return;
            }

            SocialAccount socialAccount = null;
            using (var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions { IsTransactional = false }))
            {
                using (CurrentUnitOfWork.SetSiteId(siteId))
                {
                    socialAccount = _socialAccountRepo.FindAll().Include(t => t.SocialUser).FirstOrDefault(t => t.SocialUser.OriginalId == "855320911989194753" && t.SocialUser.Type == SocialUserType.Twitter && t.IfEnable);
                    uow.Complete();
                }
            }

            ITwitterCredentials creds =
                    new TwitterCredentials("Mj6zNyYU0GGHcdAqAHv5q0oHi", "FBPUNsy5HYUdz4cRTFIST0FA0EBxi0bMPwCvae9KtIOxHenbn4",
                    socialAccount.Token, socialAccount.TokenSecret);
            var stream = Stream.CreateUserStream(creds);

            stream.StreamIsReady += (sender, args) =>
            {
                Console.WriteLine($"Stream is ready...");
            };

            stream.MessageReceived += async (sender, args) =>
            {
                await _twitterAppService.ProcessDirectMessage(socialAccount, args.Message);
            };

            stream.MessageSent += async (sender, args) =>
            {
                await _twitterAppService.ProcessDirectMessage(socialAccount, args.Message);
            };

            stream.TweetCreatedByAnyone += async (sender, args) =>
            {
                await _twitterAppService.ProcessTweet(socialAccount, args.Tweet);
            };

            stream.StreamStopped += (sender, args) =>
            {
                Console.WriteLine($"Stream is stopped...");
            };

            await stream.StartStreamAsync();
            Console.Read();
        }
    }
}
