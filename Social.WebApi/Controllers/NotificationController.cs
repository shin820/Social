using Framework.WebApi;
using Microsoft.AspNet.SignalR;
using Social.Application.AppServices;
using Social.WebApi.Hubs;
using System;
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
        private IConversationMessageAppService _messageAppService;
        public NotificationController(
            IConversationAppService conversationAppService,
            IConversationMessageAppService messageAppService
            )
        {
            _conversationAppService = conversationAppService;
            _messageAppService = messageAppService;
        }

        [Route("conversation-created")]
        [HttpGet]
        public IHttpActionResult ConversationCreated(int conversationId)
        {
            var dto = _conversationAppService.Find(conversationId);
            if (dto != null)
            {
                _hub.Clients.Group(Request.GetSiteId().ToString()).conversationCreated(dto);
            }
            return Ok();
        }

        [Route("conversation-updated")]
        [HttpGet]
        public IHttpActionResult ConversationUpdated(int conversationId)
        {
            var dto = _conversationAppService.Find(conversationId);
            if (dto != null)
            {
                _hub.Clients.Group(Request.GetSiteId().ToString()).conversationUpdated(dto);
            }
            return Ok();
        }

        [Route("facebook-comment-created")]
        [HttpGet]
        public IHttpActionResult FacebookCommentMessageCreated(int messageId)
        {
            var dto = _messageAppService.GetFacebookPostCommentMessage(messageId);
            if (dto != null)
            {
                _hub.Clients.Group(dto.ConversationId.ToString()).facebookCommentCreated(dto);
            }
            return Ok();
        }

        [Route("facebook-message-created")]
        [HttpGet]
        public IHttpActionResult FacebookMessageCreated(int messageId)
        {
            var dto = _messageAppService.GetFacebookDirectMessage(messageId);
            if (dto != null)
            {
                _hub.Clients.Group(dto.ConversationId.ToString()).facebookMessageCreated(dto);
            }
            return Ok();
        }

        [Route("twitter-tweet-created")]
        [HttpGet]
        public IHttpActionResult TwitterTweetCreated(int messageId)
        {
            var dto = _messageAppService.GetTwitterTweetMessage(messageId);
            if (dto != null)
            {
                _hub.Clients.Group(dto.ConversationId.ToString()).twitterTweetCreated(dto);
            }
            return Ok();
        }

        [Route("twitter-direct-message-created")]
        [HttpGet]
        public IHttpActionResult TwitterDirectMessageCreated(int messageId)
        {
            var dto = _messageAppService.GetTwitterDirectMessage(messageId);
            if (dto != null)
            {
                _hub.Clients.Group(dto.ConversationId.ToString()).twitterDirectMessageCreated(dto);
            }
            return Ok();
        }
    }
}