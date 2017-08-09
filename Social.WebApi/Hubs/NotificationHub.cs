using Microsoft.AspNet.SignalR;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Social.Application.Dto;
using AutoMapper;

namespace Social.WebApi.Hubs
{
    public class NotificationHub : Hub
    {
        //public void ConversationCreated(Conversation conversation)
        //{
        //    ConversationDto dto = Mapper.Map<ConversationDto>(conversation);
        //    Clients.Group(conversation.SiteId.ToString()).conversationCreated(dto);
        //}

        //public void ConversationUpdated(Conversation conversation)
        //{
        //    ConversationDto dto = Mapper.Map<ConversationDto>(conversation);
        //    Clients.Group(conversation.SiteId.ToString()).conversationUpdated(dto);
        //}

        public Task JoinConversation(int conversationId)
        {
            return Groups.Add(Context.ConnectionId, conversationId.ToString());
        }

        public Task LeaveConversation(int conversationId)
        {
            return Groups.Remove(Context.ConnectionId, conversationId.ToString());
        }

        public override Task OnConnected()
        {
            Groups.Add(Context.ConnectionId, GetSiteId());
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Groups.Remove(Context.ConnectionId, GetSiteId());
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Groups.Add(Context.ConnectionId, GetSiteId());
            return base.OnReconnected();
        }

        private string GetSiteId()
        {
            return Context.QueryString["siteId"];
        }
    }
}