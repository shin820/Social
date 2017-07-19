using AutoMapper;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;

namespace Social.Application
{
    public class DtoMappings : Profile
    {
        public DtoMappings()
        {
            CreateMap<Conversation, ConversationDto>().ForMember(dto => dto.LastMessageSenderName, opt =>
            {
                opt.MapFrom(s => s.LastMessageSender.Name);
            });
            CreateMap<ConversationCreateDto, Conversation>();
            CreateMap<FilterCreateDto, Filter>();
            CreateMap<Filter, FilterDto>();
            CreateMap<FilterConditionCreateDto, FilterCondition>();
            CreateMap<FilterCondition, FilterConditionCreateDto>();
            CreateMap<FilterUpdateDto, Filter>();
            CreateMap<FilterConditionDto, FilterCondition>();
            CreateMap<FilterCondition, FilterConditionDto>();
            CreateMap<ConversationUpdateDto, Conversation>();
            CreateMap<ConversationDto, Conversation>();
            CreateMap<ConversationField, ConversationFieldDto>();
            CreateMap<ConversationFieldOption, ConversationFieldOptionDto>();
        }
    }
}
