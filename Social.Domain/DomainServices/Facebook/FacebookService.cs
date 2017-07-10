using Facebook;
using Framework.Core;
using Social.Domain.DomainServices.Facebook;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public interface IFacebookService : ITransient
    {
        Task ProcessWebHookData(FbHookData fbData);
    }

    public class FacebookService : IFacebookService
    {
        private IRepository<SocialAccount> _socialAccountRepo;
        private IConversationStrategyFactory _strategyFactory;

        public FacebookService(
            IRepository<SocialAccount> socialAccountRepo,
            IConversationStrategyFactory strategyFactory
            )
        {
            _socialAccountRepo = socialAccountRepo;
            _strategyFactory = strategyFactory;
        }

        public async Task ProcessWebHookData(FbHookData fbData)
        {
            if (fbData == null || !fbData.Entry.Any())
            {
                return;
            }

            var changes = fbData.Entry.First().Changes;
            if (changes == null || !changes.Any())
            {
                return;
            }

            if (fbData.Object != "page")
            {
                return;
            }

            string pageId = fbData.Entry.First().Id;
            var socialAccount = _socialAccountRepo.FindAll().FirstOrDefault(t => t.SocialUser.SocialId == pageId);
            foreach (var change in changes)
            {
                if (socialAccount != null)
                {
                    var strategory = _strategyFactory.Create(change);
                    if (strategory != null)
                    {
                        await strategory.Process(socialAccount, change);
                    }
                }
            }
        }

        public static async Task<SocialUser> GetUserInfo(string token, string fbUserId, string fbUserEmail)
        {
            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbUserId + "?fields=id,name,first_name,last_name,picture,gender,email,location";
            dynamic userInfo = await client.GetTaskAsync(url);

            var user = new SocialUser
            {
                Name = userInfo.name,
                SocialId = fbUserId,
                Email = fbUserEmail
            };

            //if (userInfo.picture != null && userInfo.picture.data.url != null)
            //{
            //    user.Avatar = userInfo.picture.data.url;
            //}

            return user;
        }

        public async static Task<Message> GetMessageFromPostId(string token, string fbPostId)
        {
            Checker.NotNullOrWhiteSpace(token, nameof(token));
            Checker.NotNullOrWhiteSpace(fbPostId, nameof(fbPostId));

            FacebookClient client = new FacebookClient(token);
            string url = "/" + fbPostId + "?fields=fields=id,message,created_time,to{id,name,pic,username},from";

            dynamic post = await client.GetTaskAsync(url);
            var message = new Message
            {
                SocialId = post.id,
                SendTime = Convert.ToDateTime(post.created_time).ToUniversalTime(),
                SenderSocialId = post.from.id,
                Source = MessageSource.FacebookPost,
                Content = post.message
            };

            return message;
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
                        Id = share.id,
                        Url = share.link,
                    };

                    message.Attachments.Add(messageShare);
                }
            }

            return message;
        }
    }
}
