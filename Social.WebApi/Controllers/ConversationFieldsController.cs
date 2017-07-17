using Social.Application.AppServices;
using Social.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("ConversationFields")]
    public class ConversationFieldsController : ApiController
    {
        private IConversationFieldService _appService;

        public ConversationFieldsController(IConversationFieldService appService)
        {
            _appService = appService;
        }

        [Route()]
        public List<ConversationFieldDto> GetConversations()
        {          
            return _appService.FindAll();
        }
    }
}