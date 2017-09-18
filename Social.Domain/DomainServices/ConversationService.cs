﻿using Framework.Core;
using LinqKit;
using Social.Domain.Entities;
using Social.Domain.Entities.General;
using Social.Infrastructure;
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
        Conversation Find(int id, ConversationSource source);
        Conversation Find(int id, ConversationSource[] sources);
        IQueryable<Conversation> FindAll(Filter filter);
        Conversation GetTwitterDirectMessageConversation(SocialUser sender, SocialUser recipient);
        Conversation GetTwitterTweetConversation(string orignalTweetId);
        void AddConversation(SocialAccount socialAccount, Conversation conversation);
        IQueryable<Conversation> ApplyFilter(IQueryable<Conversation> conversations, int? filterId);
        IQueryable<Conversation> ApplyFilter(IQueryable<Conversation> conversations, Filter filter);
        IQueryable<Conversation> ApplyKeyword(IQueryable<Conversation> conversations, string keyword);
        IQueryable<Conversation> ApplySenderOrReceiverId(IQueryable<Conversation> conversations, int? userId);
        Conversation CheckIfExists(int id);
        Conversation GetUnClosedConversation(string originalId);
        Conversation Take(Conversation entity);
        Conversation Close(Conversation entity);
        Conversation Reopen(Conversation entity);
        Conversation MarkAsRead(Conversation entity);
        Conversation MarkAsUnRead(Conversation entity);
        int GetUnReadConversationCount(IList<Filter> filters);
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

        public Conversation CheckIfExists(int id)
        {
            var conversation = Find(id);
            int checkNum = CheckIfDeleteOrExists(id);
            if (checkNum == 0)
            {
                throw SocialExceptions.ConversationIdNotExists(id);
            }
            else if (checkNum == -1)
            {
                throw SocialExceptions.ConversationIdDelete(id);
            }
            return conversation;
        }

        public Conversation Find(int id, ConversationSource[] sources)
        {
            return FindAll().Where(t => t.Id == id && !t.IsHidden && sources.Contains(t.Source)).FirstOrDefault();
        }

        public Conversation Find(int id, ConversationSource source)
        {
            return FindAll().Where(t => t.Id == id && !t.IsHidden && t.Source == source).FirstOrDefault();
        }

        public int CheckIfDeleteOrExists(int id)
        {
            var conversation = FindAll().Where(t => t.Id == id ).FirstOrDefault();
            if(conversation == null)
            {
                return 0;//Not Exist
            }
            else if(conversation.IsDeleted)
            {
                return -1;//Is delete
            }
            return 1;//normal
        }

        public override Conversation Find(int id)
        {
            return Repository.FindAll().Where(t => t.Id == id && t.IsDeleted == false).FirstOrDefault();
        }

        public override IQueryable<Conversation> FindAll()
        {
            return Repository.FindAll().AsExpandable().Where(t => t.IsDeleted == false);
        }

        public Conversation GetUnClosedConversation(string originalId)
        {
            var conversations = FindAll().Where(t => t.OriginalId == originalId && t.Status != ConversationStatus.Closed);
            return conversations.FirstOrDefault();
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

        public IQueryable<Conversation> ApplySenderOrReceiverId(IQueryable<Conversation> conversations, int? userId)
        {
            if (userId != null)
            {
                conversations = conversations.Where(t => t.Messages.Any(m => m.SenderId == userId || m.ReceiverId == userId));
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
                else
                {
                    throw SocialExceptions.BadRequest($"filterId '{filterId}' not exists.");
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

        public int GetUnReadConversationCount(IList<Filter> filters)
        {
            if (filters == null || !filters.Any())
            {
                return 0;
            }

            var predicate = PredicateBuilder.New<Conversation>();
            foreach (var filter in filters)
            {
                var expression = _filterExpressionFactory.Create(filter);
                predicate.Or(expression);
            }

            var conversations = Repository.FindAll().AsExpandable().Where(t => t.IsDeleted == false && t.IfRead == false);
            return conversations.Where(predicate).Count();
        }

        public IQueryable<Conversation> FindAll(Filter filter)
        {
            var conversations = Repository.FindAll().AsExpandable().Where(t => t.IsDeleted == false && t.IfRead == false);
            var expression = _filterExpressionFactory.Create(filter);
            conversations = conversations.Where(expression);

            return conversations;
        }

        /// <summary>
        /// Get un-closed conversation whichi source is twitter direct message.
        /// </summary>
        /// <param name="user">the social user who is not a integration account and invoiced in the conversation.</param>
        /// <returns></returns>
        public Conversation GetTwitterDirectMessageConversation(SocialUser sender, SocialUser recipient)
        {
            return Repository.FindAll().Where(t => t.Source == ConversationSource.TwitterDirectMessage && t.Status != ConversationStatus.Closed && t.Messages.Any(m => (m.SenderId == sender.Id && m.ReceiverId == recipient.Id) || (m.SenderId == recipient.Id && m.ReceiverId == sender.Id))).FirstOrDefault();
        }

        public Conversation GetTwitterTweetConversation(string orignalTweetId)
        {
            if (string.IsNullOrEmpty(orignalTweetId))
            {
                return null;
            }

            return Repository.FindAll().Where(t => t.Source == ConversationSource.TwitterTweet && t.Messages.Any(m => m.OriginalId == orignalTweetId)).FirstOrDefault();
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
            var oldEntity = Repository.FindAsNoTracking().FirstOrDefault(t => t.Id == entity.Id);
            if (oldEntity == null)
            {
                return;
            }

            CheckStatus(oldEntity, entity);
            WriteConversationLog(oldEntity, entity);
            base.Update(entity);
        }

        public Conversation Take(Conversation entity)
        {
            entity.AgentId = UserContext.UserId;
            this.Update(entity);

            return entity;
        }

        public Conversation Close(Conversation entity)
        {
            entity.Status = ConversationStatus.Closed;
            this.Update(entity);

            return entity;
        }

        public Conversation Reopen(Conversation entity)
        {
            entity.Status = ConversationStatus.PendingInternal;
            this.Update(entity);

            return entity;
        }

        public Conversation MarkAsRead(Conversation entity)
        {
            entity.IfRead = true;
            this.Update(entity);

            return entity;
        }

        public Conversation MarkAsUnRead(Conversation entity)
        {
            entity.IfRead = false;
            this.Update(entity);

            return entity;
        }

        private void CheckStatus(Conversation oldEntity, Conversation conversation)
        {
            if (conversation.Source == ConversationSource.FacebookMessage || conversation.Source == ConversationSource.TwitterDirectMessage)
            {
                if (oldEntity.Status == ConversationStatus.Closed && conversation.Status != ConversationStatus.Closed)
                {
                    List<int> senderIds = oldEntity.Messages.Where(t => t.Sender.SocialAccount == null).Select(t => t.SenderId).Distinct().ToList();
                    List<int> recipientIds = oldEntity.Messages.Where(t => t.Sender.SocialAccount == null && t.ReceiverId != null).Select(t => t.ReceiverId.Value).Distinct().ToList();
                    var userIds = senderIds.Union(recipientIds).Distinct();

                    bool isExistsOpenConversation = FindAll().Any(t => t.Id != conversation.Id && t.Status != ConversationStatus.Closed && t.Messages.Any(m => userIds.Contains(m.SenderId) || userIds.Contains(m.ReceiverId.Value)));
                    if (isExistsOpenConversation)
                    {
                        throw SocialExceptions.BadRequest("Another open conversation which belongs to the same user has been found.");
                    }
                }
            }
        }

        private void WriteConversationLog(Conversation oldEntity, Conversation conversation)
        {
            if (UserContext.UserId <= 0)
            {
                return;
            }

            string agent = _agentService.GetDisplayName(UserContext.UserId);

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
                string oldAgentName = oldEntity.AgentId != null ? _agentService.GetDisplayName(oldEntity.AgentId) : "null";
                string newAgentName = conversation.AgentId != null ? _agentService.GetDisplayName(conversation.AgentId) : "null";
                WriteLog(conversation, ConversationLogType.ChangeAgentAssignee, $"Agent {agent} changed Agent Assignee from {oldAgentName} to {newAgentName}");
            }

            if (conversation.DepartmentId != oldEntity.DepartmentId)
            {
                string oldDepartmentName = oldEntity.DepartmentId != null ? _departmentService.GetDisplayName(oldEntity.DepartmentId) : "null";
                string newDepartmentName = conversation.DepartmentId != null ? _departmentService.GetDisplayName(conversation.DepartmentId) : "null";
                WriteLog(conversation, ConversationLogType.ChangeDepartmentAssignee, $"Agent {agent} changed Department Assignee from {oldDepartmentName} to {newDepartmentName}");
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
