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
        private IRepository<ConversationLog> _logRepo;
        private IAgentService _agentService;
        private IDepartmentService _departmentService;

        public ConversationService(
            IAgentService agentService,
            IDepartmentService departmentService,
            IRepository<Filter> filterRepo,
            IFilterExpressionFactory filterExpressionFactory,
            IRepository<ConversationLog> logRepo
            )
        {
            _filterRepo = filterRepo;
            _filterExpressionFactory = filterExpressionFactory;
            _logRepo = logRepo;
            _agentService = agentService;
            _departmentService = departmentService;
        }

        public override Conversation Find(int id)
        {
            return Repository.FindAll().Where(t => t.IsDeleted == false).FirstOrDefault(t => t.Id == id);
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

        public override void Update(Conversation entity)
        {
            WriteConversationLog(entity);
            base.Update(entity);
        }

        private void WriteConversationLog(Conversation conversation)
        {
            if (UserContext.UserId <= 0)
            {
                return;
            }

            var oldEntity = Repository.FindAsNoTracking().FirstOrDefault(t => t.Id == conversation.Id);
            if (oldEntity == null)
            {
                return;
            }

            string agent = _agentService.GetDiaplyName(UserContext.UserId);

            if (conversation.Priority != oldEntity.Priority)
            {
                WriteLog(conversation, ConversationLogType.ChangePriority, $"Agent {agent} changed Priority from {oldEntity.Priority.GetName()} to {conversation.Priority.GetName()}.");
            }

            if (conversation.Status != oldEntity.Status)
            {
                WriteLog(conversation, ConversationLogType.ChangeStatus, $"Agent {agent} changed Status from {oldEntity.Status.GetName()} to {conversation.Status.GetName()}.");
            }

            if (conversation.AgentId != oldEntity.AgentId)
            {
                WriteLog(conversation, ConversationLogType.ChangeAgentAssignee, $"Agent {agent} changed Agent Assignee from {_agentService.GetDiaplyName(oldEntity.AgentId)} to {_agentService.GetDiaplyName(conversation.AgentId)}.");
            }

            if (conversation.DepartmentId != oldEntity.DepartmentId)
            {
                WriteLog(conversation, ConversationLogType.ChangeDepartmentAssignee, $"Agent {agent} changed Department Assignee from {_departmentService.GetDisplayName(oldEntity.DepartmentId)} to {_departmentService.GetDisplayName(conversation.DepartmentId)}.");
            }

            if (conversation.Note != oldEntity.Note)
            {
                WriteLog(conversation, ConversationLogType.ChangeNote, $"Agent {agent} updated Note.");
            }

            if (conversation.Subject != oldEntity.Subject)
            {
                WriteLog(conversation, ConversationLogType.ChangeSubject, $"Agent {agent} updated Subject.");
            }
        }

        private void WriteLog(Conversation conversation, ConversationLogType type, string message)
        {
            conversation.Logs.Add(new ConversationLog
            {
                Type = type,
                Content = message,
            });
        }
    }
}
