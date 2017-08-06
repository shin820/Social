using Framework.WebApi;
using Microsoft.AspNet.SignalR;
using Social.Application.AppServices;
using Social.WebApi.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/notifications")]
    public class NotificationController : ApiController
    {
        private Lazy<IHubContext> _hubLazy = new Lazy<IHubContext>(() => GlobalHost.ConnectionManager.GetHubContext<NotificationHub>());

        private IHubContext _hub
        {
            get { return _hubLazy.Value; }
        }

        private IConversationAppService _conversationAppService;
        public NotificationController(IConversationAppService conversationAppService)
        {
            _conversationAppService = conversationAppService;
        }

        [Route("conversation/{conversationId}/creation")]
        [HttpGet]
        public IHttpActionResult ConversationCreation(int conversationId)
        {
            var conversationDto = _conversationAppService.Find(conversationId);
            _hub.Clients.Group(Request.GetSiteId().ToString()).conversationCreated(conversationDto);
            return Ok();
        }

        [Route("conversation/{conversationId}/modification")]
        [HttpGet]
        public IHttpActionResult ConversationModification(int conversationId)
        {
            var conversationDto = _conversationAppService.Find(conversationId);
            _hub.Clients.Group(Request.GetSiteId().ToString()).conversationModified(conversationDto);
            return Ok();
        }
    }
}