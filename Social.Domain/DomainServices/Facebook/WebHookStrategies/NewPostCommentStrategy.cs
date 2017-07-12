using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using Social.Infrastructure.Enum;

namespace Social.Domain.DomainServices.Facebook
{
    public class NewCommentStrategy : WebHookStrategy
    {
        public override bool IsMatch(FbHookChange change)
        {
            return change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.CommentId != null
                && change.Value.Item == "comment"
                && change.Value.Verb == "add";
        }

        public async override Task Process(SocialAccount socialAccount, FbHookChange change)
        {
            if (IsDuplicatedMessage(socialAccount.SiteId, change.Value.CommentId))
            {
                return;
            }

            FbMessage fbMessage = await FbClient.GetMessageFromCommentId(socialAccount.Token, change.Value.PostId, change.Value.CommentId);
            SocialUser sender = await GetOrCreateSocialUser(socialAccount.SiteId, socialAccount.Token, fbMessage.SenderId, fbMessage.SenderEmail);

            var conversation = GetConversation(socialAccount.SiteId, change.Value.PostId);
            if (conversation == null)
            {
                // todo : add log
                return;
            }

            Message message = Convert(fbMessage, sender, socialAccount);
            FillParentId(change.Value.PostId, message, fbMessage, socialAccount);

            message.ConversationId = conversation.Id;
            conversation.IfRead = false;
            conversation.Messages.Add(message);
            conversation.Status = ConversationStatus.PendingInternal;
            conversation.LastMessageSenderId = message.SenderId;
            conversation.LastMessageSentTime = message.SendTime;

            // if comment a Wall Post, the Wall Post conversation should be visible.
            if (conversation.Source == ConversationSource.FacebookWallPost && message.SenderId != socialAccount.SocialUserId && conversation.IsHidden)
            {
                conversation.IsHidden = false;
            }

            await UpdateConversation(conversation);
        }

        private void FillParentId(string postId, Message message, FbMessage fbMessage, SocialAccount socialAccount)
        {
            if (fbMessage.ParentId == null)
            {
                return;
            }

            Message parent = GetMessage(socialAccount.SiteId, fbMessage.ParentId);
            if (parent != null)
            {
                message.ParentId = parent.Id;
            }
        }

        private Message Convert(FbMessage fbMessage, SocialUser Sender, SocialAccount account)
        {
            Message message = new Message
            {
                SenderId = Sender.Id,
                Source = MessageSource.FacebookPostComment,
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
    }
}
