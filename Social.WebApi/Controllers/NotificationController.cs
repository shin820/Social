using Framework.WebApi;
using Microsoft.AspNet.SignalR;
using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Infrastructure.Enum;
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

        [Route("conversation-created")]
        [HttpPost]
        public IHttpActionResult ConversationCreated(ConversationDto conversationDto)
        {
            _hub.Clients.Group(Request.GetSiteId().ToString()).conversationCreated(conversationDto);
            return Ok();
        }

        [Route("conversation-updated")]
        [HttpPost]
        public IHttpActionResult ConversationUpdated(ConversationDto conversationDto)
        {
            _hub.Clients.Group(Request.GetSiteId().ToString()).conversationUpdated(conversationDto);
            return Ok();
        }

        [Route("facebook-comment-created")]
        [HttpPost]
        public IHttpActionResult FacebookCommentMessageCreated(FacebookPostCommentMessageDto messageDto)
        {
            _hub.Clients.Group(Request.GetSiteId().ToString()).facebookCommentCreated(messageDto);
            return Ok();
        }

        [Route("facebook-message-created")]
        [HttpPost]
        public IHttpActionResult FacebookMessageCreated(FacebookMessageDto messageDto)
        {
            _hub.Clients.Group(Request.GetSiteId().ToString()).facebookMessageCreated(messageDto);
            return Ok();
        }

        [Route("twitter-tweet-created")]
        [HttpPost]
        public IHttpActionResult TwitterTweetCreated(TwitterTweetMessageDto messageDto)
        {
            _hub.Clients.Group(Request.GetSiteId().ToString()).twitterTweetCreated(messageDto);
            return Ok();
        }

        [Route("twitter-direct-message-created")]
        [HttpPost]
        public IHttpActionResult TwitterDirectMessageCreated(TwitterDirectMessageDto messageDto)
        {
            _hub.Clients.Group(Request.GetSiteId().ToString()).twitterDirectMessageCreated(messageDto);
            return Ok();
        }
    }
}