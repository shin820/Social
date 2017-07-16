using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Events;

namespace Social.Domain.DomainServices
{
    public interface ITwitterService
    {

    }

    public class TwitterService : ServiceBase
    {
        public async Task ReceivedTweet(TweetReceivedEventArgs eventArgs)
        {
            //eventArgs.Tweet.CreatedBy.
        }
    }
}
