using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices
{
    public interface IMessageService : IDomainService<Message>
    {
        Message GetTwitterTweetMessage(string originalId);
        bool IsDuplicatedMessage(MessageSource messageSource, string originalId);
        bool IsDupliatedTweet(ITweet tweet);
    }

    public class MessageService : DomainService<Message>, IMessageService
    {
        public Message GetTwitterTweetMessage(string originalId)
        {
            return Repository.FindAll().Where(t => t.OriginalId == originalId && (t.Source == MessageSource.TwitterTypicalTweet || t.Source == MessageSource.TwitterQuoteTweet)).FirstOrDefault();
        }

        public bool IsDuplicatedMessage(MessageSource messageSource, string originalId)
        {
            return Repository.FindAll().Any(t => t.Source == messageSource && t.OriginalId == originalId);
        }

        public bool IsDupliatedTweet(ITweet tweet)
        {
            bool isQuoteTweet = tweet.QuotedTweet != null;

            if (isQuoteTweet)
            {
                return this.IsDuplicatedMessage(MessageSource.TwitterQuoteTweet, tweet.IdStr);
            }
            else
            {
                return this.IsDuplicatedMessage(MessageSource.TwitterTypicalTweet, tweet.IdStr);
            }
        }
    }
}
