using System.Threading.Tasks;
using Social.Infrastructure.Facebook;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System.Collections.Generic;

namespace Social.Domain.DomainServices.Facebook
{
    /// <summary>
    /// Process facebook hook data when a new facebook message created.
    /// </summary>
    public class NewMessageStrategy : WebHookStrategy
    {
        public override bool IsMatch(FbHookChange change)
        {
            return change.Field == "conversations" && change.Value.ThreadId != null;
        }

        public async override Task<FacebookProcessResult> Process(SocialAccount socialAccount, FbHookChange change)
        {
            var result = new FacebookProcessResult(NotificationManager);
            if (!socialAccount.IfConvertMessageToConversation)
            {
                return result;
            }

            IList<FbMessage> fbMessages = await FbClient.GetMessagesFromConversationId(socialAccount.Token, change.Value.ThreadId, 1);
            foreach (var fbMessage in fbMessages)
            {
                if (IsDuplicatedMessage(fbMessage.Id))
                {
                    return result;
                }

                await Process(result, fbMessage, socialAccount, change);
            }

            return result;
        }

        private async Task Process(FacebookProcessResult result, FbMessage fbMessage, SocialAccount socialAccount, FbHookChange change)
        {
            SocialUser sender = await GetOrCreateFacebookUser(socialAccount.Token, fbMessage.SenderId);
            SocialUser receiver = await GetOrCreateFacebookUser(socialAccount.Token, fbMessage.ReceiverId);

            var existingConversation = GetUnClosedConversation(change.Value.ThreadId);
            if (existingConversation != null)
            {
                Message message = Convert(fbMessage, sender, receiver, socialAccount);
                message.ConversationId = existingConversation.Id;
                existingConversation.IfRead = false;
                existingConversation.Status = sender.Id != socialAccount.SocialUser.Id ? ConversationStatus.PendingInternal : ConversationStatus.PendingExternal;
                existingConversation.LastMessageSenderId = message.SenderId;
                existingConversation.LastMessageSentTime = message.SendTime;
                existingConversation.Messages.Add(message);
                await UpdateConversation(existingConversation);
                await CurrentUnitOfWork.SaveChangesAsync();

                result.WithUpdatedConversation(existingConversation);
                result.WithNewMessage(message);
            }
            else
            {
                if (sender.Id == socialAccount.Id)
                {
                    return;
                }

                Message message = Convert(fbMessage, sender, receiver, socialAccount);
                var conversation = new Conversation
                {
                    OriginalId = change.Value.ThreadId,
                    Source = ConversationSource.FacebookMessage,
                    Priority = ConversationPriority.Normal,
                    Status = ConversationStatus.New,
                    Subject = GetSubject(message.Content),
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };
                conversation.Messages.Add(message);
                await AddConversation(socialAccount, conversation);
                await CurrentUnitOfWork.SaveChangesAsync();

                result.WithNewConversation(conversation);
            }
        }

        private Message Convert(FbMessage fbMessage, SocialUser Sender, SocialUser Receiver, SocialAccount account)
        {
            Message message = new Message
            {
                SenderId = Sender.Id,
                ReceiverId = Receiver.Id,
                Source = MessageSource.FacebookMessage,
                OriginalId = fbMessage.Id,
                SendTime = fbMessage.SendTime,
                Content = fbMessage.Content
            };

            foreach (var attachment in fbMessage.Attachments)
            {
                message.Attachments.Add(new MessageAttachment
                {
                    OriginalId = attachment.Id,
                    Name = attachment.Name,
                    MimeType = attachment.MimeType,
                    Type = attachment.Type,
                    Size = attachment.Size,
                    Url = attachment.Url,
                    PreviewUrl = attachment.PreviewUrl
                });
            }

            return message;
        }
    }
}
