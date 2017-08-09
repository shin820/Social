using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace Social.Domain.DomainServices.Twitter
{
    public static class TwitterConverter
    {
        public static Entities.Message ConvertToMessage(IMessage directMsg)
        {
            var message = new Entities.Message
            {
                Source = MessageSource.TwitterDirectMessage,
                Content = directMsg.Text,
                OriginalId = directMsg.Id.ToString(),
                SendTime = directMsg.CreatedAt.ToUniversalTime()
            };

            return message;
        }

        public static Entities.Message ConvertToMessage(ITweet tweet)
        {
            var message = new Entities.Message
            {
                Source = tweet.QuotedStatusId == null ? MessageSource.TwitterTypicalTweet : MessageSource.TwitterQuoteTweet,
                OriginalId = tweet.IdStr,
                SendTime = tweet.CreatedAt.ToUniversalTime(),
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

        private static MessageAttachment ConvertToMessageAttachment(IMediaEntity media)
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

            if (media.VideoDetails != null && media.VideoDetails.Variants.Any())
            {
                var video = media.VideoDetails.Variants.FirstOrDefault(t => t.Bitrate > 0);
                if (video == null)
                {
                    video = media.VideoDetails.Variants.FirstOrDefault();
                }

                return new MessageAttachment
                {
                    Type = type,
                    Url = video.URL,
                    PreviewUrl = media.MediaURL,
                    MimeType = video.ContentType,
                    OriginalId = media.IdStr,
                    OriginalLink = media.URL
                };
            }

            return new MessageAttachment
            {
                Type = type,
                Url = media.MediaURL,
                PreviewUrl = media.MediaURL,
                MimeType = new Uri(media.MediaURL).GetMimeType(),
                OriginalId = media.IdStr,
                OriginalLink = media.URL
            };
        }

    }
}
