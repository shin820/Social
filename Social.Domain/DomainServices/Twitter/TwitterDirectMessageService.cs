using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices.Twitter
{
    public interface ITwitterDirectMessageService
    {
        Task<TwitterProcessResult> ProcessDirectMessage(SocialAccount account, IMessage directMsg);
    }

    public class TwitterDirectMessageService : ServiceBase, ITwitterDirectMessageService
    {
        private IConversationService _conversationService;
        private IMessageService _messageService;
        private ISocialUserService _socialUserService;
        private TwitterProcessResult _result;

        public TwitterDirectMessageService(
            IConversationService conversationService,
            IMessageService messageService,
            ISocialUserService socialUserService,
            INotificationManager notificationManager
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _socialUserService = socialUserService;
            _result = new TwitterProcessResult(notificationManager);
        }

        public async Task<TwitterProcessResult> ProcessDirectMessage(SocialAccount account, IMessage directMsg)
        {
            if (_messageService.IsDuplicatedMessage(MessageSource.TwitterDirectMessage, directMsg.Id.ToString()))
            {
                return _result;
            }

            bool isSendByAccount = directMsg.SenderId.ToString() == account.SocialUser.OriginalId;
            SocialUser sender = await _socialUserService.GetOrCreateTwitterUser(directMsg.Sender);
            SocialUser recipient = await _socialUserService.GetOrCreateTwitterUser(directMsg.Recipient);
            var existingConversation = _conversationService.GetTwitterDirectMessageConversation(sender, recipient);
            if (existingConversation != null)
            {
                var message = TwitterConverter.ConvertToMessage(directMsg);
                message.SenderId = sender.Id;
                message.ReceiverId = recipient.Id;
                message.ConversationId = existingConversation.Id;
                existingConversation.IfRead = false;
                bool isSendByIntegrationAccount = sender.Id == account.Id & recipient.Id != account.Id;
                existingConversation.Status = isSendByIntegrationAccount ? ConversationStatus.PendingExternal : ConversationStatus.PendingInternal;
                existingConversation.LastMessageSenderId = message.SenderId;
                existingConversation.LastMessageSentTime = message.SendTime;
                existingConversation.Messages.Add(message);
                _conversationService.Update(existingConversation);
                CurrentUnitOfWork.SaveChanges();
                _result.WithUpdatedConversation(existingConversation);
                _result.WithNewMessage(message);
            }
            else
            {
                if (sender.Id == account.SocialUser.Id)
                {
                    return _result;
                }

                var message = TwitterConverter.ConvertToMessage(directMsg);
                message.SenderId = sender.Id;
                message.ReceiverId = recipient.Id;
                var conversation = new Conversation
                {
                    OriginalId = directMsg.Id.ToString(),
                    Source = ConversationSource.TwitterDirectMessage,
                    Priority = ConversationPriority.Normal,
                    Status = ConversationStatus.New,
                    Subject = TwitterUtil.GetSubject(message.Content),
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };
                conversation.Messages.Add(message);
                _conversationService.AddConversation(account, conversation);
                CurrentUnitOfWork.SaveChanges();
                _result.WithNewConversation(conversation);
            }

            return _result;
        }
    }
}
