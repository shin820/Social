using System;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using Framework.Core;
using Social.Infrastructure.Enum;

namespace Social.Domain.DomainServices.Facebook
{
    public class NewPostStrategy : WebHookStrategy
    {
        public override bool IsMatch(FbHookChange change)
        {
            bool isTextPost = change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.Item == "post"
                && change.Value.Verb == "add";

            bool isPhotOrVideoPost = change.Field == "feed"
                && change.Value.PostId != null
                //&& change.Value.Item == "photo"
                && change.Value.Verb == "add"
                && !string.IsNullOrWhiteSpace(change.Value.Link)
                && change.Value.IsPublished;

            return isTextPost || isPhotOrVideoPost;
        }

        public override async Task Process(SocialAccount socialAccount, FbHookChange change)
        {
            FbMessage fbMessage = await FbClient.GetMessageFromPostId(socialAccount.Token, change.Value.PostId);

            SocialUser sender = await GetOrCreateSocialUser(socialAccount.SiteId, socialAccount.Token, fbMessage.SenderId, fbMessage.SenderEmail);
            SocialUser receiver = await GetOrCreateSocialUser(socialAccount.SiteId, socialAccount.Token, fbMessage.ReceiverId, fbMessage.ReceiverEmail);

            var existingConversation = GetConversation(socialAccount.SiteId, change.Value.PostId);
            if (existingConversation != null)
            {
                Message message = Convert(fbMessage, sender, receiver, socialAccount, change);
                message.ConversationId = existingConversation.Id;
                existingConversation.IfRead = false;
                existingConversation.Messages.Add(message);
                existingConversation.Status = ConversationStatus.PendingInternal;
                existingConversation.LastMessageSenderId = message.SenderId;
                existingConversation.LastMessageSentTime = message.SendTime;
                await UpdateConversation(existingConversation);
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

                if (message.SenderId == socialAccount.SocialUserId)
                {
                    conversation.Source = ConversationSource.FacebookWallPost;
                    conversation.IsHidden = true;
                }

                conversation.Messages.Add(message);
                await AddConversation(conversation);
            }
        }

        private Message Convert(FbMessage fbMessage, SocialUser Sender, SocialUser Receiver, SocialAccount account, FbHookChange change)
        {
            Message message = new Message
            {
                SenderId = Sender.Id,
                ReceiverId = Receiver.Id,
                Source = MessageSource.FacebookPost,
                SocialId = fbMessage.Id,
                SendTime = fbMessage.SendTime,
                Content = fbMessage.Content,
                SiteId = account.SiteId,
                SocialLink = fbMessage.Link,
            };

            if (!string.IsNullOrWhiteSpace(change.Value.Link))
            {
                var attachment = new MessageAttachment
                {
                    SocialId = fbMessage.Id,
                    Url = change.Value.Link,
                    MimeType = new Uri(change.Value.Link).GetMimeType(),
                    SiteId = account.SiteId
                };

                message.Attachments.Add(attachment);
            }

            return message;
        }
    }
}
