using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Social.Infrastructure.Twitter
{
    public class TwitterClient
    {
        public ITweet GetTweet(long tweetId)
        {
            return Tweet.GetTweet(tweetId);
        }

        public IEnumerable<ITweet> GetTweets(long[] tweetIds)
        {
            return Tweet.GetTweets(tweetIds);
        }

        public IUser GetUser(long userId)
        {
            return User.GetUserFromId(userId);
        }

        public IEnumerable<IMessage> GetLatestMessagesReceived(int maxNumberOfMessagesRetrieve, long? maxId)
        {
            var parameter = new MessagesReceivedParameters
            {
                MaxId = maxId,
                MaximumNumberOfMessagesToRetrieve = maxNumberOfMessagesRetrieve
            };
            return Message.GetLatestMessagesReceived(parameter);
        }

        public IEnumerable<IMessage> GetLatestMessagesSent(int maxNumberOfMessagesRetrieve, long? maxId)
        {
            var parameter = new MessagesSentParameters
            {
                MaxId = maxId,
                MaximumNumberOfMessagesToRetrieve = maxNumberOfMessagesRetrieve
            };
            return Message.GetLatestMessagesSent(parameter);
        }
    }
}
