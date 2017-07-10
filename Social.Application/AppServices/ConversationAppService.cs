using AutoMapper;
using Framework.Core;
using Social.Application.Dto;
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
    }


    public class ConversationAppService : AppService, IConversationAppService
    {
        private IDomainService<Conversation> _domainService;

        public ConversationAppService(IDomainService<Conversation> domainService)
        {
            _domainService = domainService;
        }

        public PagedList<ConversationDto> Find(ConversationSearchDto searchDto)
        {
            return _domainService.FindAll().PagingAndMapping<Conversation, ConversationDto>(searchDto);
        }

        public ConversationDto Find(int id)
        {
            var conversation = _domainService.Find(id);
            return Mapper.Map<ConversationDto>(conversation);
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
    }
}
