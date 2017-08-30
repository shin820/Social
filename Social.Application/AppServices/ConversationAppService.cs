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
        ConversationDto Update(int id, ConversationUpdateDto updateDto);
        IList<ConversationLogDto> GetLogs(int converationId);
        ConversationDto Take(int conversationId);
        ConversationDto Close(int conversationId);
        ConversationDto Reopen(int conversationId);
        ConversationDto MarkAsRead(int conversationId);
        ConversationDto MarkAsUnRead(int conversationId);
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

        public ConversationDto Update(int id, ConversationUpdateDto updateDto)
        {
            Conversation conversation = _conversationService.Find(id);
            if (conversation == null)
            {
                throw SocialExceptions.ConversationIdNotExists(id);
            }
            Mapper.Map(updateDto, conversation);
            _conversationService.Update(conversation);
            _notificationManager.NotifyUpdateConversation(CurrentUnitOfWork.GetSiteId().GetValueOrDefault(), id);
            var conversationDto = Mapper.Map<ConversationDto>(conversation);
            FillFields(conversationDto);
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

        public ConversationDto Take(int conversationId)
        {
            var entity = _conversationService.Take(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(conversationDto);
            return conversationDto;
        }

        public ConversationDto Close(int conversationId)
        {
            var entity = _conversationService.Close(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(conversationDto);
            return conversationDto;
        }

        public ConversationDto Reopen(int conversationId)
        {
            var entity = _conversationService.Reopen(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(conversationDto);
            return conversationDto;
        }

        public ConversationDto MarkAsRead(int conversationId)
        {
            var entity = _conversationService.MarkAsRead(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(conversationDto);
            return conversationDto;
        }

        public ConversationDto MarkAsUnRead(int conversationId)
        {
            var entity = _conversationService.MarkAsUnRead(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(conversationDto);
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
