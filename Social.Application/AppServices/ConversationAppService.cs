using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Domain.Entities.General;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Social.Application.AppServices
{
    public interface IConversationAppService
    {
        ConversationDto Find(int id);
        IList<ConversationDto> Find(ConversationSearchDto searchDto);
        ConversationDto Insert(ConversationCreateDto createDto);
        void Delete(int id);
        Task<ConversationDto> UpdateAsync(int id, ConversationUpdateDto updateDto);
        IList<ConversationLogDto> GetLogs(int converationId);
        IList<ConversationLogDto> GetNewLogs(int conversationId, int oldMaxLogId);
        Task<ConversationDto> TakeAsync(int conversationId);
        Task<ConversationDto> CloseAsync(int conversationId);
        Task<ConversationDto> ReopenAsync(int conversationId);
        Task<ConversationDto> MarkAsReadAsync(int conversationId);
        Task<ConversationDto> MarkAsUnReadAsync(int conversationId);
    }

    public class ConversationAppService : AppService, IConversationAppService
    {
        private IConversationService _conversationService;
        private IMessageService _messageService;

        private IAgentService _agentService;
        private IDepartmentService _departmentService;
        private IDomainService<ConversationLog> _logService;
        private INotificationManager _notificationManager;

        public ConversationAppService(
            IConversationService conversationService,
            IMessageService messageService,
            IAgentService agentService,
            IDepartmentService departmentService,
            IDomainService<ConversationLog> logService,
            INotificationManager notificationManager
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _agentService = agentService;
            _departmentService = departmentService;
            _logService = logService;
            _notificationManager = notificationManager;
        }

        public IList<ConversationDto> Find(ConversationSearchDto dto)
        {
            if (dto.Since == null && dto.Until == null)
            {
                dto.Until = DateTime.UtcNow;
                dto.Since = DateTime.UtcNow.AddMonths(-3);
            }
            var conversations = _conversationService.FindAll();
            conversations = conversations.WhereIf(dto.Since != null, t => t.CreatedTime > dto.Since);
            conversations = conversations.WhereIf(dto.Until != null, t => t.CreatedTime <= dto.Until);
            conversations = _conversationService.ApplyFilter(conversations, dto.FilterId);
            conversations = _conversationService.ApplyKeyword(conversations, dto.Keyword);
            conversations = _conversationService.ApplySenderOrReceiverId(conversations, dto.UserId);

            List<ConversationDto> conversationDtos = conversations.Paging(dto).ProjectTo<ConversationDto>().ToList();
            FillFiledsForDtoList(conversationDtos);

            return conversationDtos;
        }

        private void FillFiledsForDtoList(IList<ConversationDto> conversationDtos)
        {
            var allMessages = _messageService.FindAllByConversationIds(conversationDtos.Select(t => t.Id).ToArray()).ToList();
            var agents = _agentService.Find(conversationDtos.Where(t => t.AgentId.HasValue).Select(t => t.AgentId.Value)).ToList();
            var departments = _departmentService.Find(conversationDtos.Where(t => t.DepartmentId.HasValue).Select(t => t.DepartmentId.Value)).ToList();

            foreach (var conversationDto in conversationDtos)
            {
                var messages = allMessages.Where(t => t.ConversationId == conversationDto.Id).ToList();
                FillFields(conversationDto, agents, departments, messages);
            }
        }

        public ConversationDto Find(int id)
        {
            var conversation = _conversationService.Find(id);
            if (conversation == null)
            {
                throw SocialExceptions.ConversationIdNotExists(id);
            }

            var conversationDto = Mapper.Map<ConversationDto>(conversation);
            FillFields(conversationDto);
            return conversationDto;

        }

        public ConversationDto Insert(ConversationCreateDto createDto)
        {
            var conversation = Mapper.Map<Conversation>(createDto);
            conversation = _conversationService.Insert(conversation);
            CurrentUnitOfWork.SaveChanges();
            var conversationDto = Mapper.Map<ConversationDto>(conversation);
            FillFields(conversationDto);
            return conversationDto;
        }

        public void Delete(int id)
        {
            var conversation = _conversationService.Find(id);
            if (conversation == null)
            {
                throw SocialExceptions.ConversationIdNotExists(id);
            }
            _conversationService.Delete(conversation);
        }

        public async Task<ConversationDto> UpdateAsync(int id, ConversationUpdateDto updateDto)
        {
            ConversationDto conversationDto;
            int oldMaxLogId;
            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                Conversation conversation = _conversationService.Find(id);
                if (conversation == null)
                {
                    throw SocialExceptions.ConversationIdNotExists(id);
                }
                oldMaxLogId = conversation.Logs.Max(t => t.Id);
                Mapper.Map(updateDto, conversation);
                _conversationService.Update(conversation);
                conversationDto = Mapper.Map<ConversationDto>(conversation);
                FillFields(conversationDto);
                uow.Complete();
            }
            await _notificationManager.NotifyUpdateConversation(CurrentUnitOfWork.GetSiteId().GetValueOrDefault(), id, oldMaxLogId);
            return conversationDto;
        }

        public IList<ConversationLogDto> GetLogs(int converationId)
        {
            return _logService.FindAll()
                .Where(t => t.ConversationId == converationId)
                .OrderByDescending(t => t.CreatedTime)
                .ProjectTo<ConversationLogDto>()
                .ToList();
        }

        public IList<ConversationLogDto> GetNewLogs(int conversationId, int oldMaxLogId)
        {
            return _logService.FindAll().Where(t => t.ConversationId == conversationId && t.Id > oldMaxLogId)
                .ProjectTo<ConversationLogDto>()
                .ToList();
        }

        public async Task<ConversationDto> TakeAsync(int conversationId)
        {
            return await UpdateConversation(conversationId, _conversationService.Take);
        }

        public async Task<ConversationDto> CloseAsync(int conversationId)
        {
            return await UpdateConversation(conversationId, _conversationService.Close);
        }

        public async Task<ConversationDto> ReopenAsync(int conversationId)
        {
            return await UpdateConversation(conversationId, _conversationService.Reopen);
        }

        public async Task<ConversationDto> MarkAsReadAsync(int conversationId)
        {
            return await UpdateConversation(conversationId, _conversationService.MarkAsRead);
        }

        public async Task<ConversationDto> MarkAsUnReadAsync(int conversationId)
        {
            return await UpdateConversation(conversationId, _conversationService.MarkAsUnRead);
        }

        private async Task<ConversationDto> UpdateConversation(int conversationId, Func<Conversation, Conversation> updateFunc)
        {
            int maxLogId;
            ConversationDto conversationDto;
            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                var conversation = _conversationService.Find(conversationId);
                if (conversation == null)
                {
                    throw SocialExceptions.ConversationIdNotExists(conversationId);
                }
                maxLogId = conversation.Logs.Max(t => t.Id);
                var entity = updateFunc(conversation);

                conversationDto = Mapper.Map<ConversationDto>(entity);
                FillFields(conversationDto);
                uow.Complete();
            }
            await _notificationManager.NotifyUpdateConversation(CurrentUnitOfWork.GetSiteId().GetValueOrDefault(), conversationId, maxLogId);
            return conversationDto;
        }

        private void FillFields(ConversationDto conversationDto)
        {
            List<Agent> agents = new List<Agent>();
            if (conversationDto.AgentId.HasValue)
            {
                var agent = _agentService.Find(conversationDto.AgentId.Value);
                if (agent != null)
                {
                    agents.Add(agent);
                }
            }

            List<Department> departments = new List<Department>();
            if (conversationDto.DepartmentId.HasValue)
            {
                var department = _departmentService.Find(conversationDto.DepartmentId.Value);
                if (department != null)
                {
                    departments.Add(department);
                }
            }

            var messages = _messageService
                .FindAllByConversationId(conversationDto.Id).ToList();

            FillFields(conversationDto, agents, departments, messages);
        }

        private void FillFields(ConversationDto dto, IList<Agent> agents, IList<Department> departments, IList<Message> messages)
        {
            if (dto == null)
            {
                return;
            }

            if (dto.AgentId.HasValue && agents != null && agents.Any())
            {
                var agent = _agentService.Find(dto.AgentId.Value);
                dto.AgentName = agent?.Name;
            }

            if (dto.DepartmentId.HasValue && departments != null && departments.Any())
            {
                var department = _departmentService.Find(dto.DepartmentId.Value);
                dto.DepartmentName = department?.Name;
            }

            if (messages != null && messages.Any())
            {
                messages = messages.OrderByDescending(t => t.Id).ToList();
                dto.LastMessage = messages.First().Content;
                dto.OriginalLink = messages.Last().OriginalLink;

                var lastMessageSendByCustomer = messages.FirstOrDefault(t => t.Sender.IsCustomer);
                if (lastMessageSendByCustomer != null)
                {
                    dto.CustomerId = lastMessageSendByCustomer.SenderId;
                    dto.CustomerName = lastMessageSendByCustomer.Sender.ScreenNameOrNormalName;
                    dto.CustomerAvatar = lastMessageSendByCustomer.Sender.Avatar;
                }

                var lastMessageByIntegrationAccount = messages.FirstOrDefault(t => t.IntegrationAccount != null);
                if (lastMessageByIntegrationAccount != null)
                {
                    dto.LastIntegrationAccountId = lastMessageByIntegrationAccount.IntegrationAccountId;
                    dto.LastIntegrationAccountName = lastMessageByIntegrationAccount.IntegrationAccount.ScreenNameOrNormalName;
                    dto.LastIntegrationAccountAvatar = lastMessageByIntegrationAccount.IntegrationAccount.Avatar;
                }
            }
        }
    }
}
