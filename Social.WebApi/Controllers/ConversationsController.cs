using Framework.Core;
using Social.Application.AppServices;
using Social.Application.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/conversations")]
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

        [Route("{conversationId}/logs")]
        public IList<ConversationLogDto> GetLogs(int conversationId)
        {
            return _conversationAppService.GetLogs(conversationId);
        }

        [Route("{conversationId}/facebook-messages")]
        public IList<FacebookMessageDto> GetFacebookMessages(int conversationId)
        {
            return _messageAppService.GetFacebookDirectMessages(conversationId);
        }

        [Route("{conversationId}/facebook-messages")]
        public IHttpActionResult PostFacebookMessages(int conversationId, [Required] string message)
        {
            _messageAppService.ReplyFacebookMessage(conversationId, message);
            return Ok();
        }

        [Route("{conversationId}/facebook-post-messages")]
        public FacebookPostMessageDto GetFacebookPostMessages(int conversationId)
        {
            return _messageAppService.GetFacebookPostMessages(conversationId);
        }

        [Route("{conversationId}/facebook-post-messages")]
        public IHttpActionResult PostFacebookPostMessages(int conversationId, [Required] string message, [Required] int parenId)
        {
            _messageAppService.ReplyFacebookPostOrComment(conversationId, parenId, message);
            return Ok();
        }

        [Route("{conversationId}/twitter-direct-messages")]
        public IList<TwitterDirectMessageDto> GetTwitterDirectMessages(int conversationId)
        {
            return _messageAppService.GetTwitterDirectMessages(conversationId);
        }

        [Route("{conversationId}/twitter-direct-messages")]
        public IHttpActionResult PostTwitterDirectMessages(int conversationId, [Required]string message, [Required]int twitterAccountId)
        {
            _messageAppService.ReplyTwitterDirectMessage(conversationId, twitterAccountId, message);
            return Ok();
        }

        [Route("{conversationId}/twitter-tweet-messages")]
        public IList<TwitterTweetMessageDto> GetTwitterTweetMessages(int conversationId)
        {
            return _messageAppService.GetTwitterTweetMessages(conversationId);
        }

        [Route("{conversationId}/twitter-tweet-messages")]
        public IHttpActionResult PostTwitterTweetMessages(int conversationId, [Required]string message, [Required]int twitterAccountId)
        {
            _messageAppService.ReplyTwitterTweetMessage(conversationId, twitterAccountId, message);
            return Ok();
        }
    }
}