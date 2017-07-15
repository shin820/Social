using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.Facebook
{
    public static class FacebookConverter
    {
        public static Message ConvertToMessage(string token, FbPost post)
        {
            var message = new Message
            {
                Source = MessageSource.FacebookPost,
                OriginalId = post.id,
                SendTime = post.created_time,
                Content = post.message,
                OriginalLink = post.permalink_url,
                Story = post.story
            };

            if (post.attachments != null)
            {
                foreach (var attachment in post.attachments.data)
                {
                    if (attachment.media != null && attachment.media.image != null)
                    {
                        message.Attachments.Add(ConvertToAttachment(attachment));
                    }
                }
            }

            return message;
        }

        public static Message ConvertToMessage(string token, FbComment comment)
        {
            Message message = new Message
            {
                Source = MessageSource.FacebookPostComment,
                OriginalId = comment.id,
                SendTime = comment.created_time,
                Content = comment.message,
                OriginalLink = comment.permalink_url,
            };

            if (comment.attachment != null && comment.attachment.media != null)
            {
                message.Attachments.Add(ConvertToAttachment(comment.attachment));
            }

            return message;
        }

        public static MessageAttachment ConvertToAttachment(FbAttachment attachment)
        {
            if (attachment.type == "photo" || attachment.type == "sticker")
            {
                return new MessageAttachment
                {
                    OriginalLink = attachment.url,
                    Url = attachment.media.image.src,
                    Type = MessageAttachmentType.Image,
                    MimeType = new Uri(attachment.media.image.src).GetMimeType()
                };
            }
            if (attachment.type.Contains("animated_image"))
            {
                return new MessageAttachment
                {
                    OriginalLink = attachment.url,
                    PreviewUrl = attachment.media.image.src,
                    Url = attachment.url,
                    Type = MessageAttachmentType.AnimatedImage,
                    MimeType = new Uri(attachment.url).GetMimeType()
                };
            }

            if (attachment.type.Contains("video"))
            {
                return new MessageAttachment
                {
                    OriginalLink = attachment.url,
                    PreviewUrl = attachment.media.image.src,
                    Url = attachment.url,
                    Type = MessageAttachmentType.Video,
                    MimeType = new Uri(attachment.url).GetMimeType()
                };
            }

            return new MessageAttachment
            {
                OriginalLink = attachment.url,
                Url = attachment.url,
                Type = MessageAttachmentType.File,
                MimeType = new Uri(attachment.url).GetMimeType()
            };
        }
    }
}
