using Facebook;
using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure.Facebook
{
    public static class FbClient
    {
        public static async Task<FbUser> GetUserInfo(string token, string fbUserId, string fbUserEmail)
        {
            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbUserId + "?fields=id,name,first_name,last_name,picture,gender,email,location";
            dynamic userInfo = await client.GetTaskAsync(url);

            var user = new FbUser
            {
                Id = fbUserId,
                Name = userInfo.name,
                Email = fbUserEmail
            };

            //if (userInfo.picture != null && userInfo.picture.data.url != null)
            //{
            //    user.Avatar = userInfo.picture.data.url;
            //}

            return user;
        }

        public async static Task<FbMessage> GetLastMessageFromConversationId(string token, string fbConversationId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbConversationId, nameof(fbConversationId));

            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbConversationId + "?fields=messages.limit(1){from,to,message,id,created_time,attachments,shares{link,name,id}}";
            dynamic conversation = await client.GetTaskAsync(url);
            dynamic fbMessage = conversation.messages.data[0];
            dynamic receiver = fbMessage.to.data[0];

            var message = new FbMessage
            {
                Id = fbMessage.id,
                SendTime = Convert.ToDateTime(fbMessage.created_time).ToUniversalTime(),
                SenderId = fbMessage.from.id,
                SenderEmail = fbMessage.from.email,
                ReceiverId = receiver.id,
                ReceiverEmail = receiver.email,
                Content = fbMessage.message
            };

            if (fbMessage.attachments != null)
            {
                foreach (dynamic attachmnent in fbMessage.attachments.data)
                {
                    var messageAttachment = new FbMessageAttachment
                    {
                        Id = attachmnent.id,
                        MimeType = attachmnent.mime_type,
                        Name = attachmnent.name,
                        Size = attachmnent.size,
                        Url = attachmnent.file_url
                    };
                    if (attachmnent.image_data != null)
                    {
                        messageAttachment.Url = attachmnent.image_data.url;
                        messageAttachment.PreviewUrl = attachmnent.image_data.preview_url;
                    }

                    message.Attachments.Add(messageAttachment);
                }
            }

            if (fbMessage.shares != null)
            {
                foreach (dynamic share in fbMessage.shares.data)
                {
                    var messageShare = new FbMessageAttachment
                    {
                        Name = share.name,
                        Id = share.id,
                        Url = share.link,
                    };

                    if (!string.IsNullOrWhiteSpace(messageShare.Url) && string.IsNullOrWhiteSpace(messageShare.MimeType))
                    {
                        Uri uri = new Uri(messageShare.Url);
                        messageShare.MimeType = uri.GetMimeType();
                    }

                    message.Attachments.Add(messageShare);
                }
            }

            return message;
        }

        public async static Task<FbMessage> GetMessageFromPostId(string token, string fbPostId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbPostId, nameof(fbPostId));

            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbPostId + "?fields=id,message,created_time,to{id,name,pic,username},from,permalink_url";

            dynamic post = await client.GetTaskAsync(url);
            var message = new FbMessage
            {
                Id = post.id,
                SendTime = Convert.ToDateTime(post.created_time).ToUniversalTime(),
                SenderId = post.from.id,
                ReceiverId = post.to.data[0].id, // multiple receivers?
                Content = post.message,
                Link = post.permalink_url
            };

            return message;
        }

        public async static Task<FbMessage> GetMessageFromCommentId(string token, string fbPostId, string fbCommentId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(token, nameof(fbPostId));
            Checker.NotNullOrWhiteSpace(fbCommentId, nameof(fbCommentId));

            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbCommentId + "?fields=id,parent{id},from,created_time,message,permalink_url";

            dynamic comment = await client.GetTaskAsync(url);
            var message = new FbMessage
            {
                Id = comment.id,
                SendTime = Convert.ToDateTime(comment.created_time).ToUniversalTime(),
                SenderId = comment.from.id,
                Content = comment.message,
                Link = comment.permalink_url
            };

            if (comment.parent != null)
            {
                message.ParentId = comment.parent.id;
            }
            else
            {
                message.ParentId = fbPostId;
            }

            return message;
        }
    }
}
