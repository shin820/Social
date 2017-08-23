using Social.Application.AppServices;
using Social.Application.Dto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Social.WebApi.Controllers
{
    /// <summary>
    /// Api about conversations.
    /// </summary>
    [RoutePrefix("api/conversations")]
    public class ConversationsController : ApiController
    {
        private IConversationAppService _conversationAppService;
        private IConversationMessageAppService _messageAppService;

        /// <summary>
        /// ConversationsController
        /// </summary>
        /// <param name="conversationAppService"></param>
        /// <param name="messageAppService"></param>
        public ConversationsController(
            IConversationAppService conversationAppService,
            IConversationMessageAppService messageAppService
            )
        {
            _conversationAppService = conversationAppService;
            _messageAppService = messageAppService;
        }

        /// <summary>
        /// Get conversations
        /// </summary>
        /// <param name="searchDto"></param>
        /// <returns></returns>
        [Route()]
        public IList<ConversationDto> GetConversations([FromUri(Name = "")]ConversationSearchDto searchDto)
        {
            searchDto = searchDto ?? new ConversationSearchDto();
            return _conversationAppService.Find(searchDto);
        }

        /// <summary>
        /// Get conversation by id.
        /// </summary>
        /// <param name="id">conversation id</param>
        /// <returns></returns>
        [Route("{id}", Name = "GetConversation")]
        public ConversationDto GetConversation(int id)
        {
            return _conversationAppService.Find(id);
        }

        //[Route()]
        //[ResponseType(typeof(ConversationDto))]
        //public IHttpActionResult PostConversation(ConversationCreateDto createDto)
        //{
        //    createDto = createDto ?? new ConversationCreateDto();
        //    var conversation = _conversationAppService.Insert(createDto);

        //    return CreatedAtRoute("GetConversation", new { id = conversation.Id }, conversation);
        //}

        //[Route("{id}")]
        //public IHttpActionResult DeleteConversation(int id)
        //{
        //    _conversationAppService.Delete(id);
        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        /// <summary>
        /// Update conversation.
        /// </summary>
        /// <param name="id">conversation id</param>
        /// <param name="createDto"></param>
        /// <returns></returns>
        [Route("{id}")]
        public ConversationDto PutConversation(int id, ConversationUpdateDto createDto)
        {
            createDto = createDto ?? new ConversationUpdateDto();
            return _conversationAppService.Update(id, createDto);
        }

        /// <summary>
        /// Assign agent assignee to current agent.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}/take")]
        [HttpPut]
        public ConversationDto TakeConversation(int id)
        {
            return _conversationAppService.Take(id);
        }

        /// <summary>
        /// Reopen conversation.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}/reopen")]
        [HttpPut]
        public ConversationDto ReopenConversation(int id)
        {
            return _conversationAppService.Reopen(id);
        }

        /// <summary>
        /// Close conversation.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}/close")]
        [HttpPut]
        public ConversationDto CloseConversation(int id)
        {
            return _conversationAppService.Close(id);
        }

        /// <summary>
        /// Mark conversation as Read.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}/mark-as-read")]
        [HttpPut]
        public ConversationDto MarkAsRead(int id)
        {
            return _conversationAppService.MarkAsRead(id);
        }

        /// <summary>
        /// Mark conversation as UnRead.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}/mark-as-unread")]
        [HttpPut]
        public ConversationDto MarkAsUnRead(int id)
        {
            return _conversationAppService.MarkAsUnRead(id);
        }

        /// <summary>
        /// Get conversation logs.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Route("{conversationId}/logs")]
        public IList<ConversationLogDto> GetLogs(int conversationId)
        {
            return _conversationAppService.GetLogs(conversationId);
        }

        /// <summary>
        /// Get conversation messages for facebook message.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Route("{conversationId}/facebook-messages")]
        public IList<FacebookMessageDto> GetFacebookMessages(int conversationId)
        {
            return _messageAppService.GetFacebookDirectMessages(conversationId);
        }

        /// <summary>
        /// reply message for facebook message.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Route("{conversationId}/facebook-messages")]
        public FacebookMessageDto PostFacebookMessages(int conversationId, FacebookMessagesDto dto)
        {
            return _messageAppService.ReplyFacebookMessage(conversationId, dto.Message, dto.IsCloseConversation);
        }

        /// <summary>
        /// Get conversation messages for facebook post.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Route("{conversationId}/facebook-post-messages")]
        public FacebookPostMessageDto GetFacebookPostMessages(int conversationId)
        {
            return _messageAppService.GetFacebookPostMessages(conversationId);
        }

        /// <summary>
        /// reply message for facebook post/comment.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Route("{conversationId}/facebook-post-messages")]
        public FacebookPostCommentMessageDto PostFacebookPostMessages(int conversationId, FacebookPostMessagesDto dto)
        {
            return _messageAppService.ReplyFacebookPostOrComment(conversationId, dto.ReplyCommentId, dto.Message, dto.IsCloseConversation);
        }

        /// <summary>
        /// Get conversation messages for twitter direct message.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Route("{conversationId}/twitter-direct-messages")]
        public IList<TwitterDirectMessageDto> GetTwitterDirectMessages(int conversationId)
        {
            return _messageAppService.GetTwitterDirectMessages(conversationId);
        }

        /// <summary>
        /// reply message for twitter direct message.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Route("{conversationId}/twitter-direct-messages")]
        public TwitterDirectMessageDto PostTwitterDirectMessages(int conversationId, TwitterDirectMessagesDto dto)
        {
            return _messageAppService.ReplyTwitterDirectMessage(conversationId, dto.Message, dto.IsCloseConversation);
        }

        /// <summary>
        /// Get conversation messages for twitter tweet.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Route("{conversationId}/twitter-tweet-messages")]
        public IList<TwitterTweetMessageDto> GetTwitterTweetMessages(int conversationId)
        {
            return _messageAppService.GetTwitterTweetMessages(conversationId);
        }

        /// <summary>
        /// reply message for twitter tweet.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Route("{conversationId}/twitter-tweet-messages")]
        public TwitterTweetMessageDto PostTwitterTweetMessages(int conversationId, TwitterTweetMessagesDto dto)
        {
            return _messageAppService.ReplyTwitterTweetMessage(conversationId, dto.TwitterAccountId, dto.Message, dto.IsCloseConversation);
        }
    }
}