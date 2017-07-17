using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public interface IConversationService : IDomainService<Conversation>
    {
        Conversation GetTwitterDirectMessageConversation(SocialUser user);
        void AddConversation(SocialAccount socialAccount, Conversation conversation);
    }

    public class ConversationService : DomainService<Conversation>, IConversationService
    {
        /// <summary>
        /// Get un-closed conversation whichi source is twitter direct message.
        /// </summary>
        /// <param name="user">the social user who is not a integration account and invoiced in the conversation.</param>
        /// <returns></returns>
        public Conversation GetTwitterDirectMessageConversation(SocialUser user)
        {
            return Repository.FindAll().Where(t => t.Source == ConversationSource.TwitterDirectMessage && t.Status != ConversationStatus.Closed && t.Messages.Any(m => m.SenderId == user.Id || m.ReceiverId == user.Id)).FirstOrDefault();
        }

        public void AddConversation(SocialAccount socialAccount, Conversation conversation)
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

            Repository.Insert(conversation);
        }
    }
}
