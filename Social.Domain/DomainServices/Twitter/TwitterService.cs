using Framework.Core;
using Social.Domain.DomainServices.Twitter;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;
using Tweetinvi.Parameters;

namespace Social.Domain.DomainServices
{
    public interface ITwitterService
    {
        Task<TwitterProcessResult> ProcessDirectMessage(SocialAccount account, IMessage directMsg);
        Task<TwitterProcessResult> ProcessTweet(SocialAccount account, ITweet currentTweet);
        Entities.Message GetTweetMessage(SocialAccount socialAccount, long tweetId);
        ITweet GetTweet(SocialAccount socialAccount, long tweetId);
        ITweet ReplyTweet(SocialAccount socialAccount, ITweet inReplyTo, string message);
        IUser GetUser(SocialAccount socialAccount, long userId);
        IMessage PublishMessage(SocialAccount socialAccount, IUser user, string message);
    }

    public class TwitterService : ServiceBase, ITwitterService
    {
        private IConversationService _conversationService;
        private IMessageService _messageService;
        private ITwitterTweetService _tweetServcie;
        private ITwitterDirectMessageService _directMessageService;

        public TwitterService(
            IConversationService conversationService,
            IMessageService messageService,
            ISocialUserService socialUserService,
            ITwitterTweetService tweetServcie,
            ITwitterDirectMessageService directMessageService
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _tweetServcie = tweetServcie;
            _directMessageService = directMessageService;
        }

        public Entities.Message GetTweetMessage(SocialAccount account, long tweetId)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, account.Token, account.TokenSecret));
            var tweet = Tweet.GetTweet(tweetId);
            if (tweet == null)
            {
                return null;
            }

            return TwitterConverter.ConvertToMessage(tweet);
        }

        public ITweet GetTweet(SocialAccount socialAccount, long tweetId)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, socialAccount.Token, socialAccount.TokenSecret));

            return Tweet.GetTweet(tweetId);
        }

        public IUser GetUser(SocialAccount socialAccount, long userId)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, socialAccount.Token, socialAccount.TokenSecret));
            return User.GetUserFromId(userId);
        }

        public ITweet ReplyTweet(SocialAccount socialAccount, ITweet inReplyTo, string message)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, socialAccount.Token, socialAccount.TokenSecret));

            return Tweet.PublishTweet(message, new PublishTweetOptionalParameters
            {
                InReplyToTweet = inReplyTo
            });
        }

        public IMessage PublishMessage(SocialAccount socialAccount, IUser user, string message)
        {
            Auth.SetCredentials(new TwitterCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret, socialAccount.Token, socialAccount.TokenSecret));

            return Tweetinvi.Message.PublishMessage(message, user);
        }

        public async Task<TwitterProcessResult> ProcessDirectMessage(SocialAccount account, IMessage directMsg)
        {
            return await _directMessageService.ProcessDirectMessage(account, directMsg);
        }

        public async Task<TwitterProcessResult> ProcessTweet(SocialAccount currentAccount, ITweet currentTweet)
        {
            return await _tweetServcie.ProcessTweet(currentAccount, currentTweet);
        }
    }
}
