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
    public class SocialHub : Hub
    {
        public void NewConversation(Conversation conversation)
        {
            ConversationDto dto = Mapper.Map<ConversationDto>(conversation);
            Clients.Groups(Clients.Caller.siteId).conversationCreated(dto);
        }

        public void NewMessage(Conversation conversation)
        {
            ConversationDto dto = Mapper.Map<ConversationDto>(conversation);
            Clients.Groups(Clients.Caller.siteId).conversationUpdated(dto);
        }

        public override Task OnConnected()
        {
            Groups.Add(Context.ConnectionId, Clients.Caller.siteId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Groups.Remove(Context.ConnectionId, Clients.Caller.siteId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Groups.Add(Context.ConnectionId, Clients.Caller.siteId);
            return base.OnReconnected();
        }
    }
}