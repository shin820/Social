using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.Facebook
{
    public class FacebookProcessResult
    {
        private IList<Conversation> _newConversations;
        private IList<Conversation> _updatedConversations { get; set; }
        private IList<Message> _newMessages { get; set; }

        private INotificationManager _notificationManager { get; set; }

        public FacebookProcessResult(
            INotificationManager notificationManager
            )
        {
            _newConversations = new List<Conversation>();
            _updatedConversations = new List<Conversation>();
            _newMessages = new List<Message>();
            _notificationManager = notificationManager;
        }

        public void WithNewConversation(Conversation conversation)
        {
            if (_newConversations.Any(t => t.Id == conversation.Id))
            {
                return;
            }

            _newConversations.Add(conversation);
        }

        public void WithUpdatedConversation(Conversation conversation)
        {
            if (_newConversations.Any(t => t.Id == conversation.Id))
            {
                return;
            }

            if (_updatedConversations.Any(t => t.Id == conversation.Id))
            {
                return;
            }

            _updatedConversations.Add(conversation);
        }

        public void WithNewMessage(Message message)
        {
            if (_newConversations.Any(t => t.Id == message.ConversationId))
            {
                return;
            }

            if (_newMessages.Any(t => t.Id == message.Id))
            {
                return;
            }

            _newMessages.Add(message);
        }

        public async Task Notify()
        {
            await NotifyNewConversations();
            await NotifyUpdateConversations();
            await NotifyNewMessages();
        }

        private async Task NotifyNewConversations()
        {
            foreach (var newConversation in _newConversations)
            {
                await _notificationManager.NotifyNewConversation(newConversation.SiteId, newConversation.Id);
            }

            _newConversations.Clear();
        }

        private async Task NotifyUpdateConversations()
        {
            foreach (var updatedConversation in _updatedConversations)
            {
                await _notificationManager.NotifyUpdateConversation(updatedConversation.SiteId, updatedConversation.Id);
            }

            _updatedConversations.Clear();
        }

        private async Task NotifyNewMessages()
        {
            foreach (var newMessage in _newMessages.OrderBy(t => t.Id))
            {
                if (newMessage.Source == MessageSource.FacebookMessage)
                {
                    await _notificationManager.NotifyNewFacebookMessage(newMessage.SiteId, newMessage.Id);
                }
                if (newMessage.Source == MessageSource.FacebookPostComment)
                {
                    await _notificationManager.NotifyNewFacebookComment(newMessage.SiteId, newMessage.Id);
                }
            }

            _newMessages.Clear();
        }
    }
}
