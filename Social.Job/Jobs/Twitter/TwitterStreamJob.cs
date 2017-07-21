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

        public TwitterStreamJob(
            ITwitterAppService twitterAppService
            )
        {
            _twitterAppService = twitterAppService;
        }

        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            SocialAccount socialAccount = await GetTwitterSocialAccount(context);
            if (socialAccount == null)
            {
                return;
            }

            var creds = new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret,
                    socialAccount.Token, socialAccount.TokenSecret);

            var stream = Stream.CreateUserStream(creds);

            //stream.StreamIsReady += (sender, args) =>
            //{
            //    Console.WriteLine($"Stream is ready...");
            //};

            stream.MessageReceived += async (sender, args) =>
            {
                if (socialAccount.IfConvertMessageToConversation)
                {
                    Auth.SetCredentials(creds);
                    await _twitterAppService.ProcessDirectMessage(socialAccount, args.Message);
                }
            };

            stream.MessageSent += async (sender, args) =>
            {
                await Task.Delay(1000);
                if (socialAccount.IfConvertMessageToConversation)
                {
                    Auth.SetCredentials(creds);
                    await _twitterAppService.ProcessDirectMessage(socialAccount, args.Message);
                }
            };

            stream.TweetCreatedByAnyone += async (sender, args) =>
            {
                await Task.Delay(1000);
                if (socialAccount.IfConvertTweetToConversation)
                {
                    Auth.SetCredentials(creds);
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
