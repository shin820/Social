using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace Social.Domain.DomainServices
{
    public interface ITwitterService
    {
        Task ReceivedTweet(SocialAccount account, ITweet currentTweet);
    }

    public class TwitterService : ServiceBase, ITwitterService
    {
        public IRepository<Conversation> ConversationRepository { get; set; }
        public IRepository<Entities.Message> MessageRepository { get; set; }
        public IRepository<SocialUser> SocialUserRepository { get; set; }

        public async Task ReceivedTweet(SocialAccount account, ITweet currentTweet)
        {
            bool isFristTweetSendByAccount = currentTweet.InReplyToStatusId == null && currentTweet.CreatedBy.IdStr == account.SocialUser.OriginalId;
            if (isFristTweetSendByAccount)
            {
                return;
            }

            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                using (CurrentUnitOfWork.SetSiteId(account.SiteId))
                {
                    List<ITweet> tweets = new List<ITweet>();
                    await RecursivelyFillTweet(tweets, currentTweet);
                    await AddTweets(account, tweets);
                    uow.Complete();
                }
            }
        }

        private async Task AddTweets(SocialAccount account, List<ITweet> tweets)
        {
            if (!tweets.Any())
            {
                return;
            }
            tweets = tweets.OrderBy(t => t.CreatedAt).ToList();

            foreach (var tweet in tweets)
            {
                if (tweet.InReplyToStatusId == null)
                {
                    SocialUser sender = await GetOrCreateTwitterUser(tweet.CreatedBy);
                    var message = ConvertToMessage(tweet);
                    message.SenderId = sender.Id;
                    var conversation = new Conversation
                    {
                        OriginalId = tweet.IdStr,
                        Source = ConversationSource.TwitterTweet,
                        Priority = ConversationPriority.Normal,
                        Status = ConversationStatus.New,
                        Subject = GetSubject(tweet.Text),
                        LastMessageSenderId = message.SenderId,
                        LastMessageSentTime = message.SendTime
                    };
                    conversation.Messages.Add(message);
                    await AddConversation(account, conversation);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    var inReplyToTweetMessage = MessageRepository.FindAll().Where(t => t.OriginalId == tweet.InReplyToStatusIdStr && (t.Source == MessageSource.TwitterTypicalTweet || t.Source == MessageSource.TwitterQuoteTweet)).FirstOrDefault();
                    if (inReplyToTweetMessage != null)
                    {
                        SocialUser inReplyToTweetSender = await GetOrCreateTwitterUser(tweet.CreatedBy);
                        var message = ConvertToMessage(tweet);
                        message.SenderId = inReplyToTweetSender.Id;
                        message.ParentId = inReplyToTweetMessage.Id;
                        message.ConversationId = inReplyToTweetMessage.ConversationId;
                        var conversation = inReplyToTweetMessage.Conversation;
                        conversation.Messages.Add(message);
                        conversation.IfRead = false;
                        conversation.Status = ConversationStatus.PendingInternal;
                        conversation.LastMessageSenderId = message.SenderId;
                        conversation.LastMessageSentTime = message.SendTime;
                        await ConversationRepository.UpdateAsync(conversation);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task RecursivelyFillTweet(IList<ITweet> tweets, ITweet tweet)
        {
            if (IsDupliatedTweet(tweet))
            {
                return;
            }

            tweets.Add(tweet);

            if (tweet.InReplyToStatusId != null)
            {
                ITweet inReplyToTweet = Tweet.GetTweet(tweet.InReplyToStatusId.Value);
                await RecursivelyFillTweet(tweets, inReplyToTweet);
            }
        }

        protected async Task AddConversation(SocialAccount socialAccount, Conversation conversation)
        {
            if (socialAccount.ConversationDepartmentId.HasValue)
            {
                conversation.DepartmentId = socialAccount.ConversationDepartmentId.Value;
                conversation.Priority = socialAccount.ConversationPriority ?? ConversationPriority.Normal;
            }

            if (socialAccount.ConversationAgentId.HasValue)
            {
                conversation.AgentId = socialAccount.ConversationAgentId.Value;
                conversation.Priority = socialAccount.ConversationPriority ?? ConversationPriority.Normal;
            }

            await ConversationRepository.InsertAsync(conversation);
        }

        private string GetSubject(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "No Subject";
            }

            return message.Length <= 200 ? message : message.Substring(200);
        }

        private Entities.Message ConvertToMessage(ITweet tweet)
        {
            var message = new Entities.Message
            {
                Source = tweet.QuotedStatusId == null ? MessageSource.TwitterTypicalTweet : MessageSource.TwitterQuoteTweet,
                OriginalId = tweet.IdStr,
                SendTime = tweet.CreatedAt,
                Content = tweet.Text,
                OriginalLink = tweet.Url
            };

            if (tweet.Media != null)
            {
                foreach (var media in tweet.Media)
                {
                    message.Attachments.Add(ConvertToMessageAttachment(media));
                }
            }

            return message;
        }

        private MessageAttachment ConvertToMessageAttachment(IMediaEntity media)
        {
            MessageAttachmentType type = MessageAttachmentType.File;
            if (media.MediaType == "animated_gif")
            {
                type = MessageAttachmentType.AnimatedImage;
            }
            if (media.MediaType == "photo")
            {
                type = MessageAttachmentType.Image;
            }
            if (media.MediaType == "vedio")
            {
                type = MessageAttachmentType.Video;
            }

            return new MessageAttachment
            {
                Type = type,
                Url = media.URL,
                PreviewUrl = media.MediaURL,
                OriginalId = media.IdStr,
                OriginalLink = media.URL
            };
        }

        private bool IsDupliatedTweet(ITweet tweet)
        {
            bool isQuoteTweet = tweet.QuotedTweet != null;

            if (isQuoteTweet)
            {
                return IsDuplicatedMessage(MessageSource.TwitterQuoteTweet, tweet.IdStr);
            }
            else
            {
                return IsDuplicatedMessage(MessageSource.TwitterTypicalTweet, tweet.IdStr);
            }
        }

        private bool IsDuplicatedMessage(MessageSource messageSource, string originalId)
        {
            return MessageRepository.FindAll().Any(t => t.Source == messageSource && t.OriginalId == originalId);
        }

        private async Task<SocialUser> GetOrCreateTwitterUser(IUser twitterUser)
        {
            var user = SocialUserRepository.FindAll().Where(t => t.OriginalId == twitterUser.IdStr && t.Type == SocialUserType.Twitter).FirstOrDefault();
            if (user == null)
            {
                user = new SocialUser
                {
                    OriginalId = twitterUser.IdStr,
                    Name = twitterUser.Name,
                    Type = SocialUserType.Twitter
                };
                await SocialUserRepository.InsertAsync(user);
            }
            return user;
        }
    }
}
