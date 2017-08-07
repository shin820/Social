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
        private Conversation _newConversation;
        private Conversation _updatedConversation { get; set; }
        private IList<Message> _newMessages { get; set; }

        private INotificationManager _notificationManager { get; set; }

        public FacebookProcessResult(
            INotificationManager notificationManager
            )
        {
            _newMessages = new List<Message>();
            _notificationManager = notificationManager;
        }

        public void WithNewConversation(Conversation conversation)
        {
            if (_newConversation != null)
            {
                return;
            }

            _newConversation = conversation;
        }

        public void WithUpdatedConversation(Conversation conversation)
        {
            if (_newConversation != null)
            {
                return;
            }

            if (_updatedConversation != null)
            {
                return;
            }

            _updatedConversation = conversation;
        }

        public void WithNewMessage(Message message)
        {
            if (_newConversation != null)
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
            if (_newConversation != null)
            {
                await _notificationManager.NotifyNewConversation(_newConversation.SiteId, _newConversation.Id);
            }
            if (_updatedConversation != null)
            {
                await _notificationManager.NotifyUpdateConversation(_updatedConversation.SiteId, _updatedConversation.Id);
            }

            foreach (var newMessage in _newMessages)
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
        }
    }
}
