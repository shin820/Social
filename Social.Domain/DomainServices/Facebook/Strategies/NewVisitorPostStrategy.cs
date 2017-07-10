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
    public class NewVisitorPostStrategy : IWebHookSrategy
    {
        private IRepository<Conversation> _conversationRepo;
        private IRepository<Message> _messageRepo;
        private ISocialUserInfoService _socialUserInfoService;

        public NewVisitorPostStrategy(
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
            FbMessage fbMessage = await FbClient.GetMessageFromPostId(socialAccount.Token, change.Value.PostId);

            SocialUser sender = await _socialUserInfoService.GetOrCreateSocialUser(socialAccount.SiteId, socialAccount.Token, fbMessage.SenderId, fbMessage.SenderEmail);
            SocialUser receiver = await _socialUserInfoService.GetOrCreateSocialUser(socialAccount.SiteId, socialAccount.Token, fbMessage.ReceiverId, fbMessage.ReceiverEmail);
            var existingConversation = _conversationRepo.FindAll().Where(t => t.SiteId == socialAccount.SiteId && t.SocialId == change.Value.PostId).FirstOrDefault();
            if (existingConversation != null)
            {
                Message message = Convert(fbMessage, sender, receiver, socialAccount);
                message.ConversationId = existingConversation.Id;
                existingConversation.IfRead = false;
                existingConversation.Messages.Add(message);
                existingConversation.Status = ConversationStatus.PendingInternal;
                existingConversation.LastMessageSenderId = message.SenderId;
                existingConversation.LastMessageSentTime = message.SendTime;
                await _conversationRepo.UpdateAsync(existingConversation);
            }
            else
            {
                Message message = Convert(fbMessage, sender, receiver, socialAccount);
                var conversation = new Conversation
                {
                    SocialId = change.Value.PostId,
                    Source = ConversationSource.FacebookVisitorPost,
                    Priority = ConversationPriority.Normal,
                    Status = ConversationStatus.New,
                    SiteId = socialAccount.SiteId,
                    Subject = GetSubject(message.Content),
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };
                conversation.Messages.Add(message);
                await _conversationRepo.InsertAsync(conversation);
            }
        }

        private Message Convert(FbMessage fbMessage, SocialUser Sender, SocialUser Receiver, SocialAccount account)
        {
            Message message = new Message
            {
                SenderId = Sender.Id,
                ReceiverId = Receiver.Id,
                Source = MessageSource.FacebookMessage,
                SocialId = fbMessage.Id,
                SendTime = fbMessage.SendTime,
                Content = fbMessage.Content,
                SiteId = account.SiteId,
                SocialLink = fbMessage.Link,
            };

            foreach (var attachment in fbMessage.Attachments)
            {
                message.Attachments.Add(new MessageAttachment
                {
                    SocialId = attachment.Id,
                    Name = attachment.Name,
                    MimeType = attachment.MimeType,
                    Size = attachment.Size,
                    Url = attachment.Url,
                    PreviewUrl = attachment.PreviewUrl,
                    SiteId = account.SiteId
                });
            }

            return message;
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
