using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Social.WebApi.Hubs
{
    public class NotificationHub : Hub
    {
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