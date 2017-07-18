using Facebook;
using Framework.Core;
using Newtonsoft.Json;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure.Facebook
{
    public static class FbClient
    {
        public static string GetUserToken(string code, string redirectUri)
        {
            FacebookClient client = new FacebookClient();
            try
            {
                dynamic result = client.Post("oauth/access_token", new
                {
                    client_id = AppSettings.FacebookClientId,
                    client_secret = AppSettings.FacebookClientSecret,
                    redirect_uri = redirectUri,
                    code = code
                });
                return result.access_token;
            }
            catch (FacebookOAuthException ex)
            {
                throw ExceptionHelper.FacebookOauthException(ex);
            }
        }

        public static string GetAuthUrl(string redirectUri)
        {
            return $"https://www.facebook.com/v2.9/dialog/oauth?client_id={AppSettings.FacebookClientId}&redirect_uri={redirectUri}&scope=manage_pages,publish_pages,pages_messaging,pages_messaging_phone_number,read_page_mailboxes,pages_show_list";
        }


        public static async Task<IList<FbPage>> GetPages(string userToken)
        {
            FacebookClient client = new FacebookClient(userToken);
            string url = $"/me/accounts?fields=id,name,category,access_token,picture,emails";
            dynamic result = await client.GetTaskAsync(url);

            List<FbPage> pages = new List<FbPage>();
            if (result.data != null)
            {
                foreach (var item in result.data)
                {
                    var page = new FbPage
                    {
                        Id = item.id,
                        Name = item.Name,
                        Category = item.category,
                        AccessToken = item.access_token,
                    };
                    if (item.picture != null && item.picture.data != null)
                    {
                        if (item.picture.data.is_silhouette == true)
                        {
                            page.Avatar = item.picture.data.url;
                        }
                    }

                    pages.Add(page);
                }
            }

            return pages;
        }

        public static async Task<FbUser> GetMe(string token)
        {
            ServicePointManager.DnsRefreshTimeout = 0;
            var a = ServicePointManager.SecurityProtocol;
            FacebookClient client = new FacebookClient(token);
            string url = "/me?fields=id,name,first_name,last_name,picture,gender,email,location";
            dynamic result = await client.GetTaskAsync(url);

            var me = new FbUser
            {
                id = result.id,
                name = result.name
            };
            if (result.picture != null && result.picture.data != null)
            {
                if (result.picture.data.is_silhouette == true)
                {
                    me.pic = result.picture.data.url;
                }
            }

            return me;
        }

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

        public async static Task<IList<FbMessage>> GetMessagesFromConversationId(string token, string fbConversationId)
        {
            List<FbMessage> messages = new List<FbMessage>();
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbConversationId, nameof(fbConversationId));
            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbConversationId + "/messages?fields=from,to,message,id,created_time,attachments,shares{link,name,id}&limit=13";
            dynamic fbMessages = await client.GetTaskAsync(url);
            foreach (var fbMessage in fbMessages.data)
            {
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
                            Name = attachmnent.name
                        };

                        if (attachmnent.size != null)
                        {
                            messageAttachment.Size = attachmnent.size;
                        }

                        if (attachmnent.file_url != null)
                        {
                            messageAttachment.Type = MessageAttachmentType.File;
                            messageAttachment.Url = attachmnent.file_url;
                        }

                        if (attachmnent.image_data != null)
                        {
                            messageAttachment.Type = MessageAttachmentType.Image;
                            messageAttachment.Url = attachmnent.image_data.url;
                            messageAttachment.PreviewUrl = attachmnent.image_data.preview_url;
                        }

                        if (attachmnent.video_data != null)
                        {
                            messageAttachment.Type = MessageAttachmentType.Video;
                            messageAttachment.Url = attachmnent.video_data.url;
                            messageAttachment.PreviewUrl = attachmnent.video_data.preview_url;
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

                        if (string.IsNullOrWhiteSpace(messageShare.Url) && !string.IsNullOrWhiteSpace(messageShare.Name) && string.IsNullOrWhiteSpace(message.Content))
                        {
                            message.Content = messageShare.Name;
                        }

                        if (!string.IsNullOrWhiteSpace(messageShare.Url))
                        {
                            message.Attachments.Add(messageShare);
                        }
                    }
                }

                messages.Add(message);
            }

            return messages;
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
