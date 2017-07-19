using AutoMapper;
using Social.Application.Dto;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;

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


            CreateMap<AddFaceboookPageDto, SocialAccount>()
                .ForMember(dest => dest.Token, src => src.MapFrom(x => x.AccessToken))
                .ForMember(dest => dest.FacebookPageCategory, src => src.MapFrom(x => x.Category))
                .ForMember(dest => dest.FacebookSignInAs, src => src.MapFrom(x => x.SignInAs));

            CreateMap<AddFaceboookPageDto, SocialUser>()
                .ForMember(dest => dest.OriginalId, src => src.MapFrom(x => x.FacebookId))
                .ForMember(dest => dest.Type, src => src.UseValue(SocialUserType.Facebook))
                .ForMember(dest => dest.Avatar, src => src.MapFrom(x => x.Avatar))
                .ForMember(dest => dest.OriginalLink, src => src.MapFrom(x => x.Link));

            CreateMap<SocialAccount, FacebookPageListDto>()
                .ForMember(dest => dest.Category, src => src.MapFrom(x => x.FacebookPageCategory))
                .ForMember(dest => dest.SignInAs, src => src.MapFrom(x => x.FacebookSignInAs))
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.SocialUser.Name))
                .ForMember(dest => dest.Avatar, src => src.MapFrom(x => x.SocialUser.Avatar));

            CreateMap<SocialAccount, FacebookPageDto>()
                .ForMember(dest => dest.Category, src => src.MapFrom(x => x.FacebookPageCategory))
                .ForMember(dest => dest.SignInAs, src => src.MapFrom(x => x.FacebookSignInAs))
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.SocialUser.Name))
                .ForMember(dest => dest.Avatar, src => src.MapFrom(x => x.SocialUser.Avatar));

            CreateMap<UpdateFacebookPageDto, SocialAccount>();

            CreateMap<SocialAccount, TwitterAccountListDto>()
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.SocialUser.Name));
            CreateMap<SocialAccount, TwitterAccountDto>()
                .ForMember(dest => dest.Name, src => src.MapFrom(x => x.SocialUser.Name));
            CreateMap<UpdateTwitterAccountDto, SocialAccount>();

        }
    }
}
