using Social.Application.AppServices;
using Social.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/conversation-fields")]
    public class ConversationFieldsController : ApiController
    {
        private IConversationFieldService _appService;

        public ConversationFieldsController(IConversationFieldService appService)
        {
            _appService = appService;
        }

        [Route()]
        public List<ConversationFieldDto> GetConversationFields()
        {
            return _appService.FindAll();
        }

        [Route("{id}", Name = "GetConversationField")]
        public ConversationFieldDto GetConversationField(int id)
        {
            return _appService.Find(id);
        }
    }
}