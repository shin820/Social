using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
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

        public IAgentService AgentService { get; set; }
        public IDepartmentService DepartmentService { get; set; }
        public IDomainService<ConversationLog> LogService { get; set; }
        public INotificationManager NotificationManager { get; set; }

        public ConversationAppService(
            IConversationService conversationService,
            IMessageService messageService
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
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
            var lastMessages = _messageService.GetLastMessages(conversationDtos.Select(t => t.Id).ToArray());
            var agents = AgentService.Find(conversationDtos.Where(t => t.AgentId.HasValue).Select(t => t.AgentId.Value));
            var departments = DepartmentService.Find(conversationDtos.Where(t => t.DepartmentId.HasValue).Select(t => t.DepartmentId.Value));

            foreach (var conversationDto in conversationDtos)
            {
                var agent = agents?.FirstOrDefault(t => t.Id == conversationDto.AgentId);
                conversationDto.AgentName = agent?.Name;

                var department = departments?.FirstOrDefault(t => t.Id == conversationDto.DepartmentId);
                conversationDto.DepartmentName = department?.Name;

                var lastMessage = lastMessages?.FirstOrDefault(t => t.ConversationId == conversationDto.Id);
                conversationDto.LastMessage = lastMessage?.Content;
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
            NotificationManager.NotifyUpdateConversation(CurrentUnitOfWork.GetSiteId().GetValueOrDefault(), id);
            var conversationDto = Mapper.Map<ConversationDto>(conversation);
            FillFields(conversationDto);
            return conversationDto;
        }

        public IList<ConversationLogDto> GetLogs(int converationId)
        {
            return LogService.FindAll()
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
            if (conversationDto.AgentId.HasValue)
            {
                var agent = AgentService.Find(conversationDto.AgentId.Value);
                conversationDto.AgentName = agent?.Name;
            }

            if (conversationDto.DepartmentId.HasValue)
            {
                var department = DepartmentService.Find(conversationDto.DepartmentId.Value);
                conversationDto.DepartmentName = department?.Name;
            }
            var messages = _messageService.FindAll().Where(t => t.ConversationId == conversationDto.Id);
            if (messages != null && messages.Any())
            {
                conversationDto.LastMessage = messages.OrderByDescending(t => t.Id).First().Content;
            }
        }
    }
}
