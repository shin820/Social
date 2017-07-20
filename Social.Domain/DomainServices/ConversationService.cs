using Framework.Core;
using LinqKit;
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
        IQueryable<Conversation> FindAll(Filter filter);
        Conversation GetTwitterDirectMessageConversation(SocialUser user);
        Conversation GetTwitterTweetConversation(string messageId);
        void AddConversation(SocialAccount socialAccount, Conversation conversation);
        IQueryable<Conversation> ApplyFilter(IQueryable<Conversation> conversations, int? filterId);
        IQueryable<Conversation> ApplyFilter(IQueryable<Conversation> conversations, Filter filter);
        IQueryable<Conversation> ApplyKeyword(IQueryable<Conversation> conversations, string keyword);
    }

    public class ConversationService : DomainService<Conversation>, IConversationService
    {
        private IRepository<Filter> _filterRepo;
        private IFilterExpressionFactory _filterExpressionFactory;

        public ConversationService(
            IRepository<Filter> filterRepo,
            IFilterExpressionFactory filterExpressionFactory
            )
        {
            _filterRepo = filterRepo;
            _filterExpressionFactory = filterExpressionFactory;
        }

        public override IQueryable<Conversation> FindAll()
        {
            return Repository.FindAll().AsExpandable().Where(t => t.IsDeleted == false);
        }


        public IQueryable<Conversation> ApplyKeyword(IQueryable<Conversation> conversations, string keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                var predicate = PredicateBuilder.New<Conversation>();
                predicate = predicate.Or(t => t.Subject.Contains(keyword));
                predicate = predicate.Or(t => t.Note.Contains(keyword));
                predicate = predicate.Or(t => t.Messages.Any(m => m.Content.Contains(keyword) || m.Sender.Name.Contains(keyword) || m.Receiver.Name.Contains(keyword)));

                conversations = conversations.Where(predicate);
            }

            return conversations;
        }

        public IQueryable<Conversation> ApplyFilter(IQueryable<Conversation> conversations, int? filterId)
        {
            if (filterId != null)
            {
                var filter = _filterRepo.Find(filterId.Value);
                if (filter != null)
                {
                    conversations = ApplyFilter(conversations, filter);
                }
            }

            return conversations;
        }

        public IQueryable<Conversation> ApplyFilter(IQueryable<Conversation> conversations, Filter filter)
        {
            var expression = _filterExpressionFactory.Create(filter);
            conversations = conversations.Where(expression);
            return conversations;
        }

        public IQueryable<Conversation> FindAll(Filter filter)
        {
            var conversations = Repository.FindAll().AsExpandable().Where(t => t.IsDeleted == false);
            var expression = _filterExpressionFactory.Create(filter);
            conversations = conversations.Where(expression);

            return conversations;
        }

        /// <summary>
        /// Get un-closed conversation whichi source is twitter direct message.
        /// </summary>
        /// <param name="user">the social user who is not a integration account and invoiced in the conversation.</param>
        /// <returns></returns>
        public Conversation GetTwitterDirectMessageConversation(SocialUser user)
        {
            return Repository.FindAll().Where(t => t.Source == ConversationSource.TwitterDirectMessage && t.Status != ConversationStatus.Closed && t.Messages.Any(m => m.SenderId == user.Id || m.ReceiverId == user.Id)).FirstOrDefault();
        }

        public Conversation GetTwitterTweetConversation(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return null;
            }

            return Repository.FindAll().Where(t => t.Source == ConversationSource.TwitterTweet && t.Messages.Any(m => m.OriginalId == messageId)).FirstOrDefault();
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
