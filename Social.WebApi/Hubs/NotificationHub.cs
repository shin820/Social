using Microsoft.AspNet.SignalR;
using Social.Infrastructure;
using System.Threading.Tasks;
namespace Social.WebApi.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public NotificationHub()
        {
        }

        public override Task OnConnected()
        {
            var connectionManager = GetConnectionManager();
            int userId = Context.Request.User.Identity.GetUserId().GetValueOrDefault();
            int siteId = Context.Request.User.Identity.GetSiteId().GetValueOrDefault();
            connectionManager.Connect(siteId, userId, Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionManager = GetConnectionManager();
            int userId = Context.Request.User.Identity.GetUserId().GetValueOrDefault();
            int siteId = Context.Request.User.Identity.GetSiteId().GetValueOrDefault();
            connectionManager.Disconnect(siteId, userId, Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var connectionManager = GetConnectionManager();
            int userId = Context.Request.User.Identity.GetUserId().GetValueOrDefault();
            int siteId = Context.Request.User.Identity.GetSiteId().GetValueOrDefault();
            connectionManager.Reconnect(siteId, userId, Context.ConnectionId);
            return base.OnReconnected();
        }

        public INotificationConnectionManager GetConnectionManager()
        {
            return GlobalHost.DependencyResolver.GetService(typeof(INotificationConnectionManager)) as INotificationConnectionManager;
        }
    }
}