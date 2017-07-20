using AutoMapper;
using Social.Application.Dto;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application
{
    public class ConversationMessageDtoMappings : Profile
    {
        public ConversationMessageDtoMappings()
        {
            CreateMap<Message, FacebookMessageDto>()
             .ForMember(dest => dest.UserAvatar, src => src.MapFrom(x => x.Sender.Avatar))
             .ForMember(dest => dest.UserName, src => src.MapFrom(x => x.Sender.Name))
             .ForMember(dest => dest.UserEmail, src => src.MapFrom(x => x.Sender.Email));

            CreateMap<Message, FacebookPostMessageDto>()
             .ForMember(dest => dest.UserAvatar, src => src.MapFrom(x => x.Sender.Avatar))
             .ForMember(dest => dest.UserName, src => src.MapFrom(x => x.Sender.Name))
             .ForMember(dest => dest.UserEmail, src => src.MapFrom(x => x.Sender.Email));

            CreateMap<Message, FacebookPostCommentMessageDto>()
             .ForMember(dest => dest.UserAvatar, src => src.MapFrom(x => x.Sender.Avatar))
             .ForMember(dest => dest.UserName, src => src.MapFrom(x => x.Sender.Name))
             .ForMember(dest => dest.UserEmail, src => src.MapFrom(x => x.Sender.Email));

            CreateMap<MessageAttachment, MessageAttachmentDto>();
        }
    }
}
