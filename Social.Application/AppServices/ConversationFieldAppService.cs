using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{
    public interface IConversationFieldService
    {
        List<ConversationFieldDto> FindAll();
        ConversationFieldDto Find(int id);
    }
    public class ConversationFieldAppService : AppService, IConversationFieldService
    {
        private IDomainService<ConversationField> _domainService;

        public ConversationFieldAppService(IDomainService<ConversationField> domainService)
        {
            _domainService = domainService;
        }

        public List<ConversationFieldDto> FindAll()
        {
            return _domainService.FindAll().ProjectTo<ConversationFieldDto>().ToList();
        }

        public ConversationFieldDto Find(int id)
        {
            return Mapper.Map<ConversationFieldDto>(_domainService.Find(id));
        }
    }
}
