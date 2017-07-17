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
        Task ProcessDirectMessage(SocialAccount account, IMessage directMsg);
        Task ProcessTweet(SocialAccount account, ITweet currentTweet);
    }

    public class TwitterService : ServiceBase, ITwitterService
    {
        private IConversationService _conversationService;
        private IMessageService _messageService;
        private ISocialUserService _socialUserService;

        public TwitterService(
            IConversationService conversationService,
            IMessageService messageService,
            ISocialUserService socialUserService
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _socialUserService = socialUserService;
        }

        public async Task ProcessDirectMessage(SocialAccount account, IMessage directMsg)
        {
            if (_messageService.IsDuplicatedMessage(MessageSource.TwitterDirectMessage, directMsg.Id.ToString()))
            {
                return;
            }

            bool isSendByAccount = directMsg.SenderId.ToString() == account.SocialUser.OriginalId;
            SocialUser sender = await _socialUserService.GetOrCreateTwitterUser(directMsg.Sender);
            SocialUser recipient = await _socialUserService.GetOrCreateTwitterUser(directMsg.Recipient);
            SocialUser user = sender.OriginalId != account.SocialUser.OriginalId ? sender : recipient;
            var existingConversation = _conversationService.GetTwitterDirectMessageConversation(user);
            if (existingConversation != null)
            {
                var message = ConvertToMessage(directMsg);
                message.SenderId = sender.Id;
                message.ReceiverId = recipient.Id;
                message.ConversationId = existingConversation.Id;
                existingConversation.IfRead = false;
                existingConversation.Status = ConversationStatus.PendingInternal;
                existingConversation.LastMessageSenderId = message.SenderId;
                existingConversation.LastMessageSentTime = message.SendTime;
                existingConversation.Messages.Add(message);
                _conversationService.Update(existingConversation);
            }
            else
            {
                if (sender.Id == account.SocialUser.Id)
                {
                    return;
                }

                var message = ConvertToMessage(directMsg);
                message.SenderId = sender.Id;
                message.ReceiverId = recipient.Id;
                var conversation = new Conversation
                {
                    OriginalId = directMsg.Id.ToString(),
                    Source = ConversationSource.TwitterDirectMessage,
                    Priority = ConversationPriority.Normal,
                    Status = ConversationStatus.New,
                    Subject = GetSubject(message.Content),
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };
                conversation.Messages.Add(message);
                _conversationService.AddConversation(account, conversation);
            }
        }

        private Entities.Message ConvertToMessage(IMessage directMsg)
        {
            var message = new Entities.Message
            {
                Source = MessageSource.TwitterDirectMessage,
                Content = directMsg.Text,
                OriginalId = directMsg.Id.ToString(),
                SendTime = directMsg.CreatedAt
            };

            return message;
        }

        public async Task ProcessTweet(SocialAccount account, ITweet currentTweet)
        {
            bool isFirstTweetSendByAccount = currentTweet.InReplyToStatusId == null && currentTweet.CreatedBy.IdStr == account.SocialUser.OriginalId;
            if (isFirstTweetSendByAccount)
            {
                return;
            }

            List<ITweet> tweets = new List<ITweet>();

            bool isConversationExist;
            RecursivelyFillTweet(tweets, currentTweet, out isConversationExist);

            bool ifAllTweetsCreateByAccount = !isConversationExist && tweets.All(t => t.CreatedBy.IdStr == account.SocialUser.OriginalId);
            if (ifAllTweetsCreateByAccount)
            {
                return;
            }

            await AddTweets(account, tweets);
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
                var existingConversation = _conversationService.GetTwitterTweetConversation(tweet.InReplyToStatusIdStr);
                if (existingConversation == null)
                {
                    SocialUser sender = await _socialUserService.GetOrCreateTwitterUser(tweet.CreatedBy);
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
                    _conversationService.AddConversation(account, conversation);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    var inReplyToTweetMessage = _messageService.GetTwitterTweetMessage(tweet.InReplyToStatusIdStr);
                    if (inReplyToTweetMessage != null)
                    {
                        SocialUser inReplyToTweetSender = await _socialUserService.GetOrCreateTwitterUser(tweet.CreatedBy);
                        var message = ConvertToMessage(tweet);
                        message.SenderId = inReplyToTweetSender.Id;
                        message.ParentId = inReplyToTweetMessage.Id;

                        existingConversation.Messages.Add(message);
                        existingConversation.IfRead = false;
                        existingConversation.Status = ConversationStatus.PendingInternal;
                        existingConversation.LastMessageSenderId = message.SenderId;
                        existingConversation.LastMessageSentTime = message.SendTime;
                        _conversationService.Update(existingConversation);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
        }

        private void RecursivelyFillTweet(IList<ITweet> tweets, ITweet tweet, out bool isConversationExist)
        {
            isConversationExist = false;
            if (_messageService.IsDupliatedTweet(tweet))
            {
                isConversationExist = true;
                return;
            }

            tweets.Add(tweet);

            // for performance reason, we just get 10 parent tweet in the tree.
            if (tweets.Count >= 10)
            {
                return;
            }

            if (tweet.InReplyToStatusId != null)
            {
                ITweet inReplyToTweet = Tweet.GetTweet(tweet.InReplyToStatusId.Value);
                RecursivelyFillTweet(tweets, inReplyToTweet, out isConversationExist);
            }
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
                Content = string.IsNullOrWhiteSpace(tweet.Text) ? tweet.FullText : tweet.Text,
                OriginalLink = tweet.Url
            };
            if (tweet.QuotedStatusId != null)
            {
                message.QuoteTweetId = tweet.QuotedStatusIdStr;
            }

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
    }
}
