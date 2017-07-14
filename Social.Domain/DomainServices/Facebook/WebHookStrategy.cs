using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using Framework.Core;
using Social.Infrastructure.Enum;

namespace Social.Domain.DomainServices.Facebook
{
    public abstract class WebHookStrategy : IWebHookSrategy
    {
        public IRepository<Conversation> ConversationRepository { get; set; }
        public IRepository<Message> MessageRepository { get; set; }
        public IRepository<SocialUser> SocialUserRepository { get; set; }

        public abstract bool IsMatch(FbHookChange change);

        public abstract Task Process(SocialAccount socialAccount, FbHookChange change);

        protected string GetSubject(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "No Subject";
            }

            return message.Length <= 200 ? message : message.Substring(200);
        }

        protected bool IsDuplicatedMessage(int siteId, string socialId)
        {
            return MessageRepository.FindAll().Any(t => t.SiteId == siteId && t.SocialId == socialId);
        }

        protected Message GetMessage(int siteId, string socialId)
        {
            return MessageRepository.FindAll().Where(t => t.SiteId == siteId && t.SocialId == socialId).FirstOrDefault();
        }

        protected async Task DeleteMessage(Message message)
        {
            await MessageRepository.DeleteAsync(message);
        }

        protected Conversation GetConversation(int siteId, string socialId, ConversationStatus? status = null)
        {
            var conversations = ConversationRepository.FindAll().Where(t => t.SiteId == siteId && t.SocialId == socialId);
            conversations.WhereIf(status != null, t => t.Status == status.Value);

            return conversations.FirstOrDefault();
        }

        protected async Task UpdateConversation(Conversation conversation)
        {
            await ConversationRepository.UpdateAsync(conversation);
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

        protected async Task DeleteConversation(Conversation conversation)
        {
            await ConversationRepository.DeleteAsync(conversation);
        }

        protected async Task<SocialUser> GetOrCreateSocialUser(int siteId, string token, string fbUserId, string fbUserEmail)
        {
            var user = SocialUserRepository.FindAll().Where(t => t.SiteId == siteId && t.SocialId == fbUserId).FirstOrDefault();
            if (user == null)
            {
                FbUser fbUser = await FbClient.GetUserInfo(token, fbUserId);
                user = new SocialUser
                {
                    SocialId = fbUser.id,
                    Name = fbUser.name,
                    Email = fbUser.email
                };
                user.SiteId = siteId;
                await SocialUserRepository.InsertAsync(user);
            }

            //bool ifUserInfoUpdated =
            //    socialUser.Email != facebookUser.Email
            //    || socialUser.Avatar != facebookUser.Avatar;
            //if (ifUserInfoUpdated)
            //{
            //    socialUser.Email = facebookUser.Email;
            //    socialUser.Avatar = facebookUser.Avatar;
            //    await Repository.UpdateAsync(socialUser);
            //    return socialUser;
            //}

            return user;
        }
    }
}
