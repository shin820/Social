﻿using AutoMapper;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.Twitter
{
    public class TwitterProcessResult
    {
        private IList<Conversation> _newConversations;
        private IList<Conversation> _updatedConversations { get; set; }
        private IList<Message> _newMessages { get; set; }

        private INotificationManager _notificationManager { get; set; }

        public TwitterProcessResult(
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

        public async Task Notify(int siteId)
        {
            await NotifyNewConversations(siteId);
            await NotifyUpdateConversations(siteId);
            await NotifyNewMessages(siteId);
        }

        private async Task NotifyNewConversations(int siteId)
        {
            foreach (var newConversation in _newConversations)
            {
                await _notificationManager.NotifyNewConversation(siteId, newConversation.Id);
            }

            _newConversations.Clear();
        }

        private async Task NotifyUpdateConversations(int siteId)
        {
            foreach (var updatedConversation in _updatedConversations)
            {
                await _notificationManager.NotifyUpdateConversation(siteId, updatedConversation.Id);
            }

            _updatedConversations.Clear();
        }

        private async Task NotifyNewMessages(int siteId)
        {
            foreach (var newMessage in _newMessages.OrderBy(t => t.Id))
            {
                if (newMessage.Source == MessageSource.TwitterDirectMessage)
                {
                    await _notificationManager.NotifyNewTwitterDirectMessage(siteId, newMessage.Id);
                }
                if (newMessage.Source == MessageSource.TwitterQuoteTweet || newMessage.Source == MessageSource.TwitterTypicalTweet)
                {
                    await _notificationManager.NotifyNewTwitterTweet(siteId, newMessage.Id);
                }
            }

            _newMessages.Clear();
        }
    }
}
