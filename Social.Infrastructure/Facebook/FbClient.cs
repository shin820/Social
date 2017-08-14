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
        public async static Task SubscribeApp(string pageId, string pageToken)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsync($"https://graph.facebook.com/v2.9/{pageId}/subscribed_apps?access_token={pageToken}", null);
            string result = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }

        public async static Task UnSubscribeApp(string pageId, string pageToken)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.DeleteAsync($"https://graph.facebook.com/v2.9/{pageId}/subscribed_apps?access_token={pageToken}");
            string result = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }

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
                throw SocialExceptions.FacebookOauthException(ex);
            }
        }

        public static string GetAuthUrl(string redirectUri)
        {
            return $"https://www.facebook.com/v2.9/dialog/oauth?client_id={AppSettings.FacebookClientId}&redirect_uri={redirectUri}&scope=manage_pages,publish_pages,pages_messaging,pages_messaging_phone_number,read_page_mailboxes,pages_show_list";
        }


        public static async Task<IList<FbPage>> GetPages(string userToken)
        {
            FacebookClient client = new FacebookClient(userToken);
            string url = $"/me/accounts?fields=id,name,category,access_token,picture,emails,link";
            dynamic result = await client.GetTaskAsync(url);

            List<FbPage> pages = new List<FbPage>();
            if (result.data != null)
            {
                foreach (var item in result.data)
                {
                    var page = new FbPage
                    {
                        Id = item.id,
                        Name = item.name,
                        Category = item.category,
                        AccessToken = item.access_token,
                        Link = item.link
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
            FacebookClient client = new FacebookClient(token);
            string url = "/me?fields=id,name,link,picture";
            dynamic result = await client.GetTaskAsync(url);

            var me = new FbUser
            {
                id = result.id,
                name = result.name,
                link = result.link
            };
            if (result.picture != null && result.picture.data != null)
            {
                if (result.picture.data.url != null)
                {
                    me.pic = result.picture.data.url;
                }
            }

            return me;
        }

        public static async Task<FbUser> GetUserInfo(string token, string fbUserId)
        {
            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbUserId + "?fields=id,name,link,picture";

            try
            {
                dynamic result = await client.GetTaskAsync(url);

                var me = new FbUser
                {
                    id = result.id,
                    name = result.name,
                    link = result.link
                };
                if (result.picture != null && result.picture.data != null)
                {
                    if (result.picture.data.url != null)
                    {
                        me.pic = result.picture.data.url;
                    }
                }

                return me;
            }
            catch (Exception ex)
            {
                Logger.Error($"Get user info from facebook failed.token={token},url={url}", ex);
                throw ex;
            }

            //if (userInfo.picture != null && userInfo.picture.data.url != null)
            //{
            //    user.Avatar = userInfo.picture.data.url;
            //}
        }

        public static FbUser GetFacebookUserInfo(string token, string fbUserId)
        {
            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbUserId + "?fields=id,name,link,picture";

            try
            {
                dynamic result = client.Get(url);

                var me = new FbUser
                {
                    id = result.id,
                    name = result.name,
                    link = result.link
                };
                if (result.picture != null && result.picture.data != null)
                {
                    if (result.picture.data.url != null)
                    {
                        me.pic = result.picture.data.url;
                    }
                }
                if (result.email != null)
                {
                    me.email = result.email;
                }

                return me;
            }
            catch (Exception ex)
            {
                Logger.Error($"Get user info from facebook failed.token={token},url={url}", ex);
                throw ex;
            }
        }

        public static string PublishComment(string token, string parentId, string message)
        {
            FacebookClient client = new FacebookClient(token);
            string url = $"/{parentId}/comments";
            try
            {
                dynamic result = client.Post(url, new { message = message });
                return result.id;
            }
            catch (FacebookOAuthException ex)
            {
                return string.Empty;
            }
        }

        public static string PublishPost(string pageId, string token, string message)
        {
            FacebookClient client = new FacebookClient(token);
            string url = $"/{pageId}/feed";
            try
            {
                dynamic result = client.Post(url, new { message = message });
                return result.id;
            }
            catch (FacebookOAuthException ex)
            {
                return string.Empty;
            }
        }

        public static string PublishMessage(string token, string fbConversationId, string message)
        {
            FacebookClient client = new FacebookClient(token);
            string url = $"/{fbConversationId}/messages";
            dynamic result = client.Post(url, new { message = message });
            return result.id;
        }

        public async static Task<IList<FbMessage>> GetMessagesFromConversationId(string token, string fbConversationId, int limit)
        {
            List<FbMessage> messages = new List<FbMessage>();
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbConversationId, nameof(fbConversationId));
            FacebookClient client = new FacebookClient(token);
            string url = $"/{fbConversationId}/messages?fields=from,to,message,id,created_time,attachments,shares{{link,name,id}}&limit={limit}";
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
                            if (messageShare.MimeType.Contains("image"))
                            {
                                messageShare.Type = MessageAttachmentType.Image;
                            }
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

        public static FbComment GetComment(string token, string fbCommentId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbCommentId, nameof(fbCommentId));

            FacebookClient client = new FacebookClient(token);
            string url = $"/{fbCommentId}?fields=id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden";

            return client.Get<FbComment>(url);
        }

        public async static Task<FbPagingData<FbPost>> GetVisitorPosts(string pageId, string token)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(token, nameof(pageId));
            FacebookClient client = new FacebookClient(token);

            long since = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();

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

            long since = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();

            string toFields = $"to{{id,name,pic,username,profile_type,link}}";
            string innnerCommentsFields = $"comments.since({since}){{id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden}}";
            string commentFieds = $"comments.since({since}){{id,parent,from,created_time,message,permalink_url,attachment,comment_count,is_hidden,{innnerCommentsFields}}}";
            string url = $"/{pageId}/tagged?fields=id,message,created_time,from,permalink_url,story,type,status_type,link,is_hidden,is_published,attachments,updated_time,tagged_time,{toFields},{commentFieds}&since={since}";

            return await client.GetTaskAsync<FbPagingData<FbPost>>(url);
        }


        public async static Task<FbPagingData<FbConversation>> GetConversationsPosts(string pageId, string token)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(token, nameof(pageId));
            FacebookClient client = new FacebookClient(token);

            long since = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();

            string sharesFields = $"shares{{link,name,id}}";
            string messagesFields = $"messages.since({since}){{from,to,message,id,created_time,attachments,{sharesFields}}}";
            string url = $"/{pageId}/conversations?fields=id,updated_time,{messagesFields}";


            List<FbConversation> Conversations = new List<FbConversation>();
            dynamic fbConversations = await client.GetTaskAsync(url);

            FbCursors FbCursors = new FbCursors
            {
                before = fbConversations.paging.cursors.before,
                after = fbConversations.paging.cursors.after
            };
            FbPaging FbPaging = new FbPaging
            {
                cursors = FbCursors,
                next = fbConversations.paging.next,
                previous = fbConversations.paging.previous
            };

            FbPagingData<FbConversation> PagingConversation = new FbPagingData<FbConversation>();
            PagingConversation.paging = FbPaging;
            foreach (var conversation in fbConversations.data)
            {
                List<FbMessage> FbMessages = new List<FbMessage>();
                foreach (var fbMessage in conversation.messages.data)
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
                                if (messageShare.MimeType.Contains("image"))
                                {
                                    messageShare.Type = MessageAttachmentType.Image;
                                }
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

                    FbMessages.Add(message);
                }
                FbCursors messageFbCursors = new FbCursors
                {
                    before = conversation.messages.paging.cursors.before,
                    after = conversation.messages.paging.cursors.after
                };
                FbPaging messageFbPaging = new FbPaging
                {
                    cursors = messageFbCursors,
                    next = conversation.messages.paging.next,
                    previous = conversation.messages.paging.previous
                };
                FbPagingData<FbMessage> PagingFbMessage = new FbPagingData<FbMessage>
                {
                    data = FbMessages,
                    paging = messageFbPaging
                };
                FbConversation FbConversation = new FbConversation
                {
                    Id = conversation.id,
                    UpdateTime = Convert.ToDateTime(conversation.updated_time).ToUniversalTime(),
                    Messages = PagingFbMessage
                };
                Conversations.Add(FbConversation);
            }
            PagingConversation.data = Conversations;

            return PagingConversation;
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
