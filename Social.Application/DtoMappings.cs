using AutoMapper;
using Social.Application.Dto;
using Social.Domain.Entities;

namespace Social.Application
{
    public class DtoMappings : Profile
    {
        public DtoMappings()
        {
            CreateMap<Conversation, ConversationDto>();
            CreateMap<ConversationCreateDto, Conversation>();
            CreateMap<FilterCreateDto, Filter>();
            CreateMap<Filter, FilterDto>();
            CreateMap<FilterConditionCreateDto, FilterCondition>();
            CreateMap<FilterCondition,FilterConditionCreateDto>();
            CreateMap<FilterUpdateDto, Filter>();
            CreateMap<FilterConditionDto, FilterCondition>();
            CreateMap<FilterCondition, FilterConditionDto>();


        }
    }
}
