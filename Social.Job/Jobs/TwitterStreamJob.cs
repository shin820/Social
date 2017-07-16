using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Tweetinvi.Models;
using Tweetinvi;

namespace Social.Job.Jobs
{
    public class TwitterStreamJob : JobBase, ITransient
    {
        protected async override Task ExecuteJob(IJobExecutionContext context)
        {
            ITwitterCredentials creds =
                            new TwitterCredentials("Mj6zNyYU0GGHcdAqAHv5q0oHi",                            "FBPUNsy5HYUdz4cRTFIST0FA0EBxi0bMPwCvae9KtIOxHenbn4",    "855320911989194753-25EU8AmKqJw8HhPJYdCUcje2mat9UxV","HQYSviXLSEFHZkF2xqj8R9KxWRtIHG3Tp4yBdjpEutUa3");
            var stream = Stream.CreateUserStream(creds);

            stream.StreamIsReady += (sender, args) =>
            {
                Console.WriteLine($"Stream is ready...");
            };

            stream.MessageReceived += (sender, args) =>
            {
                Console.WriteLine($"[{args.Message.CreatedAt}] {args.Message.SenderScreenName} : {args.Message.Text}");
                var user = User.GetUserFromScreenName(args.Message.SenderScreenName);
                var b = user;
            };

            stream.MessageSent += (sender, args) =>
            {
                Console.WriteLine($"[{args.Message.CreatedAt}] {args.Message.SenderScreenName} : {args.Message.Text}");
            };

            stream.TweetCreatedByAnyone += (sender, args) =>
            {
                Console.WriteLine($"[{args.Tweet.CreatedAt}] {args.Tweet.CreatedBy.ScreenName} : {args.Tweet.Text}");
                var user = User.GetUserFromScreenName(args.Tweet.CreatedBy.ScreenName);
                var b = user;
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
