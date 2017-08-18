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
        private IDomainService<ConversationLog> _logService;
        private INotificationManager _notificationManager;

        public ConversationAppService(
            IConversationService conversationService,
            IDomainService<ConversationLog> logService,
            INotificationManager notificationManager,
            IMessageService messageService
            )
        {
            _conversationService = conversationService;
            _logService = logService;
            _notificationManager = notificationManager;
            _messageService = messageService;
        }

        public IList<ConversationDto> Find(ConversationSearchDto dto)
        {
            if (dto.Since == null && dto.Util == null)
            {
                dto.Util = DateTime.UtcNow;
                dto.Since = DateTime.UtcNow.AddMonths(-3);
            }
            var conversations = _conversationService.FindAll();
            conversations = conversations.WhereIf(dto.Since != null, t => t.CreatedTime > dto.Since);
            conversations = conversations.WhereIf(dto.Util != null, t => t.CreatedTime <= dto.Util);
            conversations = _conversationService.ApplyFilter(conversations, dto.FilterId);
            conversations = _conversationService.ApplyKeyword(conversations, dto.Keyword);
            conversations = _conversationService.ApplySenderOrReceiverId(conversations, dto.UserId);

            List<ConversationDto> conversationDtos = conversations.Paging(dto).ProjectTo<ConversationDto>().ToList();

            var lastMessages = _messageService.GetLastMessages(conversations.Select(t => t.Id).ToArray());
            for (int i = 0; i < conversationDtos.Count(); i++)
            {
                conversationDtos[i].AgentName = _conversationService.GetAgentName(conversations.ToArray()[i]);
                conversationDtos[i].DepartmentName = _conversationService.GetDepartmentName(conversations.ToArray()[i]);
                if (lastMessages != null && lastMessages.Any())
                {
                    var lastMessage = lastMessages.FirstOrDefault(t => t.ConversationId == conversationDtos[i].Id);
                    if (lastMessage != null)
                    {
                        conversationDtos[i].LastMessage = lastMessage.Content;
                    }
                }
            }
            return conversationDtos;
        }

        public ConversationDto Find(int id)
        {
            var conversation = _conversationService.Find(id);
            if (conversation == null)
            {
                throw SocialExceptions.ConversationIdNotExists(id);
            }

            var conversationDto = Mapper.Map<ConversationDto>(conversation);
            FillFields(conversation, conversationDto);
            return conversationDto;

        }

        public ConversationDto Insert(ConversationCreateDto createDto)
        {
            var conversation = Mapper.Map<Conversation>(createDto);
            conversation = _conversationService.Insert(conversation);
            CurrentUnitOfWork.SaveChanges();
            var conversationDto = Mapper.Map<ConversationDto>(conversation);
            FillFields(conversation, conversationDto);
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
            FillFields(conversation, conversationDto);
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
            FillFields(entity, conversationDto);
            return conversationDto;
        }

        public ConversationDto Close(int conversationId)
        {
            var entity = _conversationService.Close(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(entity, conversationDto);
            return conversationDto;
        }

        public ConversationDto Reopen(int conversationId)
        {
            var entity = _conversationService.Reopen(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(entity, conversationDto);
            return conversationDto;
        }

        public ConversationDto MarkAsRead(int conversationId)
        {
            var entity = _conversationService.MarkAsRead(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(entity, conversationDto);
            return conversationDto;
        }

        public ConversationDto MarkAsUnRead(int conversationId)
        {
            var entity = _conversationService.MarkAsUnRead(conversationId);
            var conversationDto = Mapper.Map<ConversationDto>(entity);
            FillFields(entity, conversationDto);
            return conversationDto;
        }

        private void FillFields(Conversation conversation, ConversationDto conversationDto)
        {
            conversationDto.AgentName = _conversationService.GetAgentName(conversation);
            conversationDto.DepartmentName = _conversationService.GetDepartmentName(conversation);

            var messages = _messageService.FindAll().Where(t => t.ConversationId == conversation.Id);
            if (messages != null && messages.Any())
            {
                conversationDto.LastMessage = messages.OrderByDescending(t => t.Id).First().Content;
            }
        }
    }
}
