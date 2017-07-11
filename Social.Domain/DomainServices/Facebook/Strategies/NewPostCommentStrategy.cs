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
    public class NewCommentStrategy : IWebHookSrategy
    {
        private IRepository<Conversation> _conversationRepo;
        private IRepository<Message> _messageRepo;
        private ISocialUserInfoService _socialUserInfoService;

        public NewCommentStrategy(
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
                && change.Value.CommentId != null
                && change.Value.Item == "comment"
                && change.Value.Verb == "add";
        }

        public async Task Process(SocialAccount socialAccount, FbHookChange change)
        {
            FbMessage fbMessage = await FbClient.GetMessageFromCommentId(socialAccount.Token, change.Value.PostId, change.Value.CommentId);
            SocialUser sender = await _socialUserInfoService.GetOrCreateSocialUser(socialAccount.SiteId, socialAccount.Token, fbMessage.SenderId, fbMessage.SenderEmail);

            var conversation = _conversationRepo.FindAll().Where(t => t.SiteId == socialAccount.SiteId && t.SocialId == change.Value.PostId).FirstOrDefault();

            bool isDuplicatedComment = conversation.Messages.Any(t => t.SocialId == fbMessage.Id);
            if (isDuplicatedComment)
            {
                return;
            }

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
            if (conversation.Source == ConversationSource.FacebookWallPost
                && conversation.IsHidden)
            {
                conversation.IsHidden = false;
            }

            await _conversationRepo.UpdateAsync(conversation);
        }

        private void FillParentId(string postId, Message message, FbMessage fbMessage, SocialAccount socialAccount)
        {
            if (fbMessage.ParentId == null)
            {
                return;
            }

            Message parent = _messageRepo.FindAll(t => t.SiteId == socialAccount.SiteId && t.SocialId == fbMessage.ParentId).FirstOrDefault();
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
