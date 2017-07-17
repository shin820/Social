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
    }
    public class ConversationFieldAppService: AppService,IConversationFieldService
    {
        private IDomainService<ConversationField> _domainService;

        public ConversationFieldAppService(IDomainService<ConversationField> domainService)
        {
            _domainService = domainService;
        }

        public List<ConversationFieldDto> FindAll()
        {
            List<ConversationFieldDto> ConversationFieldDtos = new List<ConversationFieldDto>();
            var conversationFields = _domainService.FindAll();
                for(int i = 0; i < conversationFields.Count();i++)
            {
                if(conversationFields.ToArray()[i].DataType == Infrastructure.Enum.FieldDataType.Option)
                {
                    List<string> options = new List<string>();
                    options = GetOptions(conversationFields.ToArray()[i].Name);               
                }
                ConversationFieldDtos.Add(Mapper.Map<ConversationFieldDto>(conversationFields.ToArray()[i]));
            }
            return _domainService.FindAll().ProjectTo<ConversationFieldDto>().ToList();
        }

        public List<string> GetOptions(string Name)
        {
            List<string> Options = new List<string>();
            List<String> EnumNames = new List<string>();
            EnumNames = Enum.GetNames(typeof(Option)).ToList();
            foreach (var EnumName in EnumNames)
            {
                DescriptionAttribute att = Attribute.GetCustomAttribute(typeof(Option).GetField(EnumName), typeof(DescriptionAttribute), false) as DescriptionAttribute;
                if (Name == att.Description)
                {
                    Options.Add(EnumName);
                }
            }
            return Options;
        }
    }
}
