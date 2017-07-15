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
        public static async Task<FbUser> GetUserInfo(string token, string fbUserId)
        {
            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbUserId + "?fields=id,name,first_name,last_name,picture,gender,email,location";

            return await client.GetTaskAsync<FbUser>(url);

            //if (userInfo.picture != null && userInfo.picture.data.url != null)
            //{
            //    user.Avatar = userInfo.picture.data.url;
            //}
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

        public async static Task<FbPost> GetPost(string token, string fbPostId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbPostId, nameof(fbPostId));

            FacebookClient client = new FacebookClient(token);
            string url = $"/{fbPostId}?fields=id,message,created_time,from,permalink_url,story,type,status_type,link,is_hidden,is_published,updated_time,attachments";

            return await client.GetTaskAsync<FbPost>(url);
        }

        public async static Task<FbComment> GetComment(string token, string fbCommentId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbCommentId, nameof(fbCommentId));

            FacebookClient client = new FacebookClient(token);
            string url = $"/{fbCommentId}?fields=id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden";

            return await client.GetTaskAsync<FbComment>(url);
        }

        public async static Task<FbMessage> GetMessageFromPostId(string token, string fbPostId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbPostId, nameof(fbPostId));

            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbPostId + "?fields=id,message,created_time,to{id,name,pic,username},from,permalink_url,story";

            dynamic post = await client.GetTaskAsync(url);
            var message = new FbMessage
            {
                Id = post.id,
                SendTime = Convert.ToDateTime(post.created_time).ToUniversalTime(),
                SenderId = post.from.id,
                Content = post.message,
                Link = post.permalink_url,
                Story = post.story
            };

            if (post.to != null)
            {
                message.ReceiverId = post.to.data[0].id;
            }

            return message;
        }

        public async static Task<FbMessage> GetMessageFromCommentId(string token, string fbPostId, string fbCommentId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(token, nameof(fbPostId));
            Checker.NotNullOrWhiteSpace(fbCommentId, nameof(fbCommentId));

            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbCommentId + "?fields=id,parent{id},from,created_time,message,permalink_url,attachment";

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

            if (comment.attachment != null && comment.attachment.media != null && comment.attachment.media.image != null)
            {
                message.Attachments.Add(new FbMessageAttachment
                {
                    Url = comment.attachment.media.image.src,
                    MimeType = new Uri(comment.attachment.media.image.src).GetMimeType()
                });
            }

            return message;
        }

        public async static Task<FbPagingData<FbPost>> GetVisitorPosts(string pageId, string token)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(token, nameof(pageId));
            FacebookClient client = new FacebookClient(token);

            long since = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeSeconds();

            string toFields = $"to{{id,name,pic,username,profile_type,link}}";
            string innnerCommentsFields = $"comments.since({since}){{id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden}}";
            string commentFieds = $"comments.since({since}){{id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden,{innnerCommentsFields}}}";
            string url = $"/{pageId}/feed?fields=id,message,created_time,from,permalink_url,story,type,status_type,link,is_hidden,is_published,attachments,updated_time,tagged_time,{toFields},{commentFieds}&since={since}";


            return await client.GetTaskAsync<FbPagingData<FbPost>>(url);
        }

        public async static Task<FbPagingData<FbPost>> GetTaggedVisitorPosts(string pageId, string token)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(token, nameof(pageId));
            FacebookClient client = new FacebookClient(token);

            long since = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeSeconds();

            string toFields = $"to{{id,name,pic,username,profile_type,link}}";
            string innnerCommentsFields = $"comments.since({since}){{id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden}}";
            string commentFieds = $"comments.since({since}){{id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden,{innnerCommentsFields}}}";
            string url = $"/{pageId}/tagged?fields=id,message,created_time,from,permalink_url,story,type,status_type,link,is_hidden,is_published,attachments,updated_time,tagged_time,{toFields},{commentFieds}&since={since}";


            return await client.GetTaskAsync<FbPagingData<FbPost>>(url);
        }


        public async static Task<FbComment> GetPostComment(string commentId, string token, int limit = 100)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(commentId));
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            FacebookClient client = new FacebookClient(token);

            string url = $"/{commentId}?fields=id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden,comments.limit({limit}){{id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden}}";

            return await client.GetTaskAsync<FbComment>(url);
        }

        public async static Task Process<T>(int siteId, string token, FbPagingData<T> fbPagingData, Func<int, string, T, Task> processFunc)
        {
            FacebookClient facebookClient = new FacebookClient(token);
            foreach (var data in fbPagingData.data)
            {
                await processFunc(siteId, token, data);
            }

            if (!string.IsNullOrEmpty(fbPagingData.paging.next))
            {
                var nextData = await facebookClient.GetTaskAsync<FbPagingData<T>>(fbPagingData.paging.next);
                await Process(siteId, token, nextData, processFunc);
            }
        }
    }
}
