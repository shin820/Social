using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.Facebook
{
    public class NewPostWithPhotoOrVideoStrategy : WebHookStrategy
    {
        private IRepository<Conversation> _conversationRepo;
        private IRepository<Message> _messageRepo;
        private ISocialUserInfoService _socialUserInfoService;

        public NewPostWithPhotoOrVideoStrategy(
            IRepository<Conversation> conversationRepo,
            IRepository<Message> messageRepo,
            ISocialUserInfoService socialUserInfoService
            )
        {
            _conversationRepo = conversationRepo;
            _messageRepo = messageRepo;
            _socialUserInfoService = socialUserInfoService;
        }

        public override bool IsMatch(FbHookChange change)
        {
            return change.Field == "feed"
                && change.Value.PostId != null
                //&& change.Value.Item == "photo"
                && change.Value.Verb == "add"
                && !string.IsNullOrWhiteSpace(change.Value.Link)
                && change.Value.IsPublished;
        }

        public async override Task Process(SocialAccount socialAccount, FbHookChange change)
        {
            FbMessage fbMessage = await FbClient.GetMessageFromPostId(socialAccount.Token, change.Value.PostId);

            SocialUser sender = await _socialUserInfoService.GetOrCreateSocialUser(socialAccount.SiteId, socialAccount.Token, fbMessage.SenderId, fbMessage.SenderEmail);
            SocialUser receiver = await _socialUserInfoService.GetOrCreateSocialUser(socialAccount.SiteId, socialAccount.Token, fbMessage.ReceiverId, fbMessage.ReceiverEmail);
            var existingConversation = _conversationRepo.FindAll().Where(t => t.SiteId == socialAccount.SiteId && t.SocialId == change.Value.PostId).FirstOrDefault();
            if (existingConversation != null)
            {
                Message message = Convert(fbMessage, sender, receiver, socialAccount, change);
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
                Message message = Convert(fbMessage, sender, receiver, socialAccount, change);
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

        private Message Convert(
            FbMessage fbMessage, SocialUser Sender, SocialUser Receiver, SocialAccount account, FbHookChange change)
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

            var attachment = new MessageAttachment
            {
                SocialId = fbMessage.Id,
                Url = change.Value.Link,
                MimeType = new Uri(change.Value.Link).GetMimeType(),
                SiteId = account.SiteId
            };

            message.Attachments.Add(attachment);

            return message;
        }
    }
}
