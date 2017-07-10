using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using Framework.Core;
using Social.Infrastructure.Enum;

namespace Social.Domain.DomainServices.Facebook
{
    public class VisitorPostStrategy : IConversationSrategy
    {
        private IRepository<Conversation> _conversationRepo;
        private IRepository<Message> _messageRepo;
        private ISocialUserInfoService _socialUserInfoService;

        public VisitorPostStrategy(
            IRepository<Conversation> conversationRepo,
            IRepository<Message> messageRepo,
            ISocialUserInfoService socialUserInfoService
            )
        {
            _conversationRepo = conversationRepo;
            _messageRepo = messageRepo;
            _socialUserInfoService = socialUserInfoService;
        }

        public bool IsMatch(FbHookChange change)
        {
            return change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.Item == "post"
                && change.Value.Verb == "add";
        }

        public async Task Process(SocialAccount socialAccount, FbHookChange change)
        {
            Message message = await FacebookService.GetMessageFromPostId(socialAccount.Token, change.Value.PostId);
            message.SiteId = socialAccount.SiteId;

            var existingConversation = _conversationRepo.FindAll().Where(t => t.SiteId == socialAccount.SiteId && t.SocialId == change.Value.PostId).FirstOrDefault();
            if (existingConversation != null)
            {
                existingConversation.IfRead = false;
                existingConversation.Messages.Add(message);
                existingConversation.Status = ConversationStatus.PendingInternal;
                existingConversation.LastMessageSenderId = message.SenderId;
                existingConversation.LastMessageSentTime = message.SendTime;
                await _conversationRepo.UpdateAsync(existingConversation);
            }
            else
            {
                var conversation = new Conversation
                {
                    SocialId = change.Value.PostId,
                    Source = ConversationSource.FacebookMessage,
                    SiteId = socialAccount.SiteId,
                    Subject = GetSubject(message.Content),
                    //SocialAccountId = socialAccount.Id,
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };
                conversation.Messages.Add(message);
                await _conversationRepo.InsertAsync(conversation);
            }
        }

        private string GetSubject(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "No Subject";
            }

            return message.Length <= 200 ? message : message.Substring(200);
        }
    }
}
