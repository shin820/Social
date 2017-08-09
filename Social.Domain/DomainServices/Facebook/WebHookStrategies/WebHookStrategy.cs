using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using Framework.Core;
using Social.Infrastructure.Enum;
using Social.Infrastructure;

namespace Social.Domain.DomainServices.Facebook
{
    public abstract class WebHookStrategy : ServiceBase, IWebHookSrategy
    {
        public IConversationService ConversationService { get; set; }
        public ISocialUserService SocialUserService { get; set; }
        public IMessageService MessageService { get; set; }
        public INotificationManager NotificationManager { get; set; }

        public abstract bool IsMatch(FbHookChange change);

        public abstract Task<FacebookProcessResult> Process(SocialAccount socialAccount, FbHookChange change);

        protected string GetSubject(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "No Subject";
            }

            return message.Length <= 200 ? message : message.Substring(200);
        }

        protected bool IsDuplicatedMessage(string originalId)
        {
            return MessageService.FindAll().Any(t => t.OriginalId == originalId);
        }

        protected bool IsDuplicatedMessage(MessageSource source, string originalId)
        {
            return MessageService.IsDuplicatedMessage(source, originalId);
        }

        protected Message GetMessage(MessageSource source, string originalId)
        {
            return MessageService.FindByOriginalId(source, originalId);
        }

        protected async Task DeleteMessage(Message message)
        {
            await MessageService.DeleteAsync(message);
        }

        protected Conversation GetConversation(string originalId)
        {
            var conversations = ConversationService.FindAll().Where(t => t.OriginalId == originalId);
            return conversations.FirstOrDefault();
        }

        protected Conversation GetUnClosedConversation(string originalId)
        {
            return ConversationService.GetUnClosedConversation(originalId);
        }

        protected async Task UpdateConversation(Conversation conversation)
        {
            //   ConversationService.Update(conversation);

            await ConversationService.UpdateAsync(conversation);
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

            await ConversationService.InsertAsync(conversation);
        }

        protected async Task DeleteConversation(Conversation conversation)
        {
            await ConversationService.DeleteAsync(conversation);
        }

        protected async Task<SocialUser> GetOrCreateFacebookUser(string token, string fbUserId)
        {
            var user = SocialUserService.FindAll().Where(t => t.OriginalId == fbUserId && t.Source == SocialUserSource.Facebook).FirstOrDefault();
            if (user == null)
            {
                FbUser fbUser = await FbClient.GetUserInfo(token, fbUserId);
                user = new SocialUser
                {
                    OriginalId = fbUser.id,
                    Name = fbUser.name,
                    Email = fbUser.email,
                    OriginalLink = fbUser.link,
                    Avatar = fbUser.pic,
                    Source = SocialUserSource.Facebook
                };

                await SocialUserService.InsertAsync(user);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            return user;
        }
    }
}
