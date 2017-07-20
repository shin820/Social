using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices
{
    public interface IMessageService : IDomainService<Message>
    {
        IQueryable<Message> FindAllByConversationId(int conversationId);
        Message GetTwitterTweetMessage(string originalId);
        bool IsDuplicatedMessage(MessageSource messageSource, string originalId);
        bool IsDupliatedTweet(ITweet tweet);
    }

    public class MessageService : DomainService<Message>, IMessageService
    {
        public IQueryable<Message> FindAllByConversationId(int conversationId)
        {
            return Repository.FindAll()
                .Include(t => t.Attachments)
                .Include(t => t.Sender)
                .Where(t => t.ConversationId == conversationId && t.IsDeleted == false);
        }

        public Message GetTwitterTweetMessage(string originalId)
        {
            return Repository.FindAll()
                .Where(t => t.OriginalId == originalId && (t.Source == MessageSource.TwitterTypicalTweet || t.Source == MessageSource.TwitterQuoteTweet))
                .FirstOrDefault();
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
