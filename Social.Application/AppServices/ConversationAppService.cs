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
        void Update(int id, ConversationUpdateDto updateDto);
        IList<ConversationLogDto> GetLogs(int converationId);
    }

    public class ConversationAppService : AppService, IConversationAppService
    {
        private IConversationService _conversationService;
        private IDomainService<ConversationLog> _logService;
        private INotificationManager _notificationManager;

        public ConversationAppService(
            IConversationService conversationService,
            IDomainService<ConversationLog> logService,
            INotificationManager notificationManager
            )
        {
            _conversationService = conversationService;
            _logService = logService;
            _notificationManager = notificationManager;
        }

        public IList<ConversationDto> Find(ConversationSearchDto dto)
        {
            if (dto.Since == null && dto.Util == null)
            {
                dto.Util = DateTime.UtcNow;
                dto.Since = DateTime.UtcNow.AddMonths(-3);
            }
            var conversations = _conversationService.FindAll();
            conversations = conversations.WhereIf(dto.Since != null, t => t.CreatedTime >= dto.Since);
            conversations = conversations.WhereIf(dto.Util != null, t => t.CreatedTime <= dto.Util);
            conversations = _conversationService.ApplyFilter(conversations, dto.FilterId);
            conversations = _conversationService.ApplyKeyword(conversations, dto.Keyword);
            conversations = _conversationService.ApplyOriginalId(conversations, dto.UserId);

            return conversations.Paging(dto).ProjectTo<ConversationDto>().ToList();
        }

        public ConversationDto Find(int id)
        {
            var conversation = _conversationService.Find(id);
            return Mapper.Map<ConversationDto>(conversation);
        }

        public ConversationDto Insert(ConversationCreateDto createDto)
        {
            var conversation = Mapper.Map<Conversation>(createDto);
            conversation = _conversationService.Insert(conversation);
            CurrentUnitOfWork.SaveChanges();
            return Mapper.Map<ConversationDto>(conversation);
        }

        public void Delete(int id)
        {
            _conversationService.Delete(id);
        }

        public void Update(int id, ConversationUpdateDto updateDto)
        {
            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                var conversationDto = _conversationService.Find(id);
                var conversation = Mapper.Map<Conversation>(conversationDto);
                Mapper.Map(updateDto, conversation);
                _conversationService.Update(conversation);
                uow.Complete();
            }
            _notificationManager.NotifyUpdateConversation(CurrentUnitOfWork.GetSiteId().GetValueOrDefault(), id);
        }

        public IList<ConversationLogDto> GetLogs(int converationId)
        {
            return _logService.FindAll()
                .Where(t => t.ConversationId == converationId)
                .OrderByDescending(t => t.CreatedTime)
                .ProjectTo<ConversationLogDto>()
                .ToList();
        }
    }
}
