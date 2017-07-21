using Framework.Core;
using System;
using System.Threading.Tasks;
using Quartz;
using Tweetinvi.Models;
using Tweetinvi;
using Social.Application.AppServices;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Domain.DomainServices;
using Social.Infrastructure;

namespace Social.Job.Jobs
{
    public class TwitterStreamJob : JobBase, ITransient
    {
        private ITwitterAppService _twitterAppService;
        private ISocialAccountService _socialAccountService;

        private ITwitterCredentials _creds;

        public TwitterStreamJob(
            ITwitterAppService twitterAppService,
            ISocialAccountService socialAccountService
            )
        {
            _twitterAppService = twitterAppService;
            _socialAccountService = socialAccountService;
        }

        protected async override Task ExecuteJob(IJobExecutionContext context)
        {

            var siteSocicalAccount = context.JobDetail.GetCustomData<SiteSocialAccount>();
            if (siteSocicalAccount == null)
            {
                return;
            }

            int siteId = siteSocicalAccount.SiteId;
            string twitterUserId = siteSocicalAccount.TwitterUserId;

            SocialAccount socialAccount = null;
            await UnitOfWorkManager.RunWithoutTransaction(siteId, async () =>
            {
                socialAccount = await _socialAccountService.GetAccountAsync(SocialUserSource.Twitter, twitterUserId);
            });

            if (socialAccount == null)
            {
                return;
            }

            _creds = new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret,
                    socialAccount.Token, socialAccount.TokenSecret);

            var stream = Stream.CreateUserStream(_creds);

            //stream.StreamIsReady += (sender, args) =>
            //{
            //    Console.WriteLine($"Stream is ready...");
            //};

            stream.MessageReceived += async (sender, args) =>
            {
                if (socialAccount.IfConvertMessageToConversation)
                {
                    Auth.SetCredentials(_creds);
                    await _twitterAppService.ProcessDirectMessage(socialAccount, args.Message);
                }
            };

            stream.MessageSent += async (sender, args) =>
            {
                if (socialAccount.IfConvertMessageToConversation)
                {
                    Auth.SetCredentials(_creds);
                    await _twitterAppService.ProcessDirectMessage(socialAccount, args.Message);
                }
            };

            stream.TweetCreatedByAnyone += async (sender, args) =>
            {
                if (socialAccount.IfConvertTweetToConversation)
                {
                    Auth.SetCredentials(_creds);
                    await _twitterAppService.ProcessTweet(socialAccount, args.Tweet);
                }
            };

            stream.StreamStopped += (sender, args) =>
            {
                Logger.Error($"Twitter User Stream stopped. JobKey={context.JobDetail.Key}.", args.Exception);
            };

            await stream.StartStreamAsync();
            Console.Read();
        }
    }
}
