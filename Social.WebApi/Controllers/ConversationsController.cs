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
        private IConversationAppService _appService;

        public ConversationsController(IConversationAppService appService)
        {
            _appService = appService;
        }

        [Route()]
        public PagedList<ConversationDto> GetConversations([FromUri(Name = "")]ConversationSearchDto searchDto)
        {
            searchDto = searchDto ?? new ConversationSearchDto();
            return _appService.Find(searchDto);
        }

        [Route("{id}", Name = "GetConversation")]
        public ConversationDto GetConversation(int id)
        {
            return _appService.Find(id);
        }

        [Route()]
        [ResponseType(typeof(ConversationDto))]
        public IHttpActionResult PostConversation(ConversationCreateDto createDto)
        {
            createDto = createDto ?? new ConversationCreateDto();
            var conversation = _appService.Insert(createDto);

            return CreatedAtRoute("GetConversation", new { id = conversation.Id }, conversation);
        }

        [Route("{id}")]
        public IHttpActionResult DeleteConversation(int id)
        {
            _appService.Delete(id);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{id}")]
        [ResponseType(typeof(ConversationDto))]
        public IHttpActionResult PutConversation(int id, ConversationUpdateDto createDto)
        {
            createDto = createDto ?? new ConversationUpdateDto();
            _appService.Update(id, createDto);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{id}/logs")]
        public IList<ConversationLogDto> GetLogs(int id)
        {
            return _appService.GetLogs(id);
        }

        [Route("{id}/facebook-messages")]
        public IList<FacebookMessageDto> GetFacebookMessages(int id)
        {
            return _appService.GetFacebookDirectMessages(id);
        }

        [Route("{id}/facebook-post-messages")]
        public FacebookPostMessageDto GetFacebookPostMessages(int id)
        {
            return _appService.GetFacebookPostMessages(id);
        }
    }
}