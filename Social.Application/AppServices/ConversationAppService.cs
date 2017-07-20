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

namespace Social.Application.AppServices
{
    public interface IConversationAppService
    {
        ConversationDto Find(int id);
        PagedList<ConversationDto> Find(ConversationSearchDto searchDto);
        ConversationDto Insert(ConversationCreateDto createDto);
        void Delete(int id);
        void Update(int id, ConversationUpdateDto updateDto);
        IList<ConversationLogDto> GetLogs(int converationId);

        IList<FacebookMessageDto> GetFacebookDirectMessages(int conversationId);
        FacebookPostMessageDto GetFacebookPostMessages(int conversationId);
    }

    public class ConversationAppService : AppService, IConversationAppService
    {
        private IConversationService _conversationService;
        private IMessageService _messageService;
        private IAgentService _agentService;
        private IDomainService<ConversationLog> _logService;

        public ConversationAppService(
            IConversationService conversationService,
            IAgentService agentService,
            IMessageService messageService,
            IDomainService<ConversationLog> logService
            )
        {
            _conversationService = conversationService;
            _agentService = agentService;
            _messageService = messageService;
            _logService = logService;
        }

        public PagedList<ConversationDto> Find(ConversationSearchDto dto)
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

            return conversations.PagingAndMapping<Conversation, ConversationDto>(dto);
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

            return Mapper.Map<ConversationDto>(conversation);
        }

        public void Delete(int id)
        {
            _conversationService.Delete(id);
        }

        public void Update(int id, ConversationUpdateDto updateDto)
        {
            var conversationDto = _conversationService.Find(id);
            var conversation = Mapper.Map<Conversation>(conversationDto);
            Mapper.Map(updateDto, conversation);
            _conversationService.Update(conversation);
        }

        public IList<ConversationLogDto> GetLogs(int converationId)
        {
            return _logService.FindAll()
                .Where(t => t.ConversationId == converationId)
                .OrderByDescending(t => t.CreatedTime)
                .ProjectTo<ConversationLogDto>()
                .ToList();
        }

        public FacebookPostMessageDto GetFacebookPostMessages(int conversationId)
        {
            var converation = _conversationService.Find
                (conversationId, new[] { ConversationSource.FacebookVisitorPost, ConversationSource.FacebookWallPost });
            if (converation == null)
            {
                return null;
            }

            var messages = _messageService.FindAllByConversationId(conversationId).ToList();
            var postMessage = messages.FirstOrDefault(t => t.Source == MessageSource.FacebookPost);
            if (postMessage == null)
            {
                return null;
            }
            var postDto = Mapper.Map<FacebookPostMessageDto>(postMessage);

            var allComments = messages.Where(t => t.Source == MessageSource.FacebookPostComment).Select(t => Mapper.Map<FacebookPostCommentMessageDto>(t)).ToList();
            _agentService.FillAgentName(allComments.Cast<IHaveSendAgent>());

            postDto.Comments = allComments.Where(t => t.ParentId == postDto.Id).ToList();
            foreach (var comment in postDto.Comments)
            {
                comment.ReplyComments = allComments.Where(t => t.ParentId == comment.Id).ToList();
            }

            return postDto;
        }

        public IList<FacebookMessageDto> GetFacebookDirectMessages(int conversationId)
        {
            List<FacebookMessageDto> result = new List<FacebookMessageDto>();

            var converation = _conversationService.Find(conversationId, ConversationSource.FacebookMessage);
            if (converation == null)
            {
                return result;
            }

            result = _messageService.FindAllByConversationId(conversationId)
                .OrderBy(t => t.SendTime)
                .ProjectTo<FacebookMessageDto>().ToList();

            _agentService.FillAgentName(result.Cast<IHaveSendAgent>());

            return result;
        }
    }
}
