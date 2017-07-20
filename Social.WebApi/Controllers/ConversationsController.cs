using Framework.Core;
using Social.Application.AppServices;
using Social.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("conversations")]
    public class ConversationsController : ApiController
    {
        private IConversationAppService _conversationAppService;
        private IConversationMessageAppService _messageAppService;

        public ConversationsController(
            IConversationAppService conversationAppService,
            IConversationMessageAppService messageAppService
            )
        {
            _conversationAppService = conversationAppService;
            _messageAppService = messageAppService;
        }

        [Route()]
        public PagedList<ConversationDto> GetConversations([FromUri(Name = "")]ConversationSearchDto searchDto)
        {
            searchDto = searchDto ?? new ConversationSearchDto();
            return _conversationAppService.Find(searchDto);
        }

        [Route("{id}", Name = "GetConversation")]
        public ConversationDto GetConversation(int id)
        {
            return _conversationAppService.Find(id);
        }

        [Route()]
        [ResponseType(typeof(ConversationDto))]
        public IHttpActionResult PostConversation(ConversationCreateDto createDto)
        {
            createDto = createDto ?? new ConversationCreateDto();
            var conversation = _conversationAppService.Insert(createDto);

            return CreatedAtRoute("GetConversation", new { id = conversation.Id }, conversation);
        }

        [Route("{id}")]
        public IHttpActionResult DeleteConversation(int id)
        {
            _conversationAppService.Delete(id);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{id}")]
        [ResponseType(typeof(ConversationDto))]
        public IHttpActionResult PutConversation(int id, ConversationUpdateDto createDto)
        {
            createDto = createDto ?? new ConversationUpdateDto();
            _conversationAppService.Update(id, createDto);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{id}/logs")]
        public IList<ConversationLogDto> GetLogs(int id)
        {
            return _conversationAppService.GetLogs(id);
        }

        [Route("{id}/facebook-messages")]
        public IList<FacebookMessageDto> GetFacebookMessages(int id)
        {
            return _messageAppService.GetFacebookDirectMessages(id);
        }

        [Route("{id}/facebook-post-messages")]
        public FacebookPostMessageDto GetFacebookPostMessages(int id)
        {
            return _messageAppService.GetFacebookPostMessages(id);
        }

        [Route("{id}/twitter-direct-messages")]
        public IList<TwitterDirectMessageDto> GetTwitterDirectMessages(int id)
        {
            return _messageAppService.GetTwitterDirectMessages(id);
        }

        [Route("{id}/twitter-tweet-messages")]
        public IList<TwitterTweetMessageDto> GetTwitterTweetMessages(int id)
        {
            return _messageAppService.GetTwitterTweetMessages(id);
        }
    }
}