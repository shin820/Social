using AutoMapper;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{
    public interface IConversationAppService
    {
        ConversationDto Find(int id);
        PagedList<ConversationDto> Find(ConversationSearchDto searchDto);
        ConversationDto Insert(ConversationCreateDto createDto);
        void Delete(int id);
        void Update(int id, ConversationUpdateDto updateDto);
    }

    public class ConversationAppService : AppService, IConversationAppService
    {
        private IConversationService _domainService;

        public ConversationAppService(IConversationService domainService)
        {
            _domainService = domainService;
        }

        public PagedList<ConversationDto> Find(ConversationSearchDto searchDto)
        {
            return _domainService.FindAll("", searchDto.FilterId).PagingAndMapping<Conversation, ConversationDto>(searchDto);
        }

        public ConversationDto Find(int id)
        {
            var conversation = _domainService.Find(id);
            if (conversation.IsDeleted == false)
            {
                return Mapper.Map<ConversationDto>(conversation);
            }
            else
            {
                return null;
            }
        }

        public ConversationDto Insert(ConversationCreateDto createDto)
        {
            var conversation = Mapper.Map<Conversation>(createDto);
            conversation = _domainService.Insert(conversation);

            return Mapper.Map<ConversationDto>(conversation);
        }

        public void Delete(int id)
        {
            _domainService.Delete(id);
        }

        public void Update(int id, ConversationUpdateDto updateDto)
        {
            var conversationDto = _domainService.Find(id);
            var conversation = Mapper.Map<Conversation>(conversationDto);
            Mapper.Map(updateDto, conversation);
            _domainService.Update(conversation);
        }
    }
}
