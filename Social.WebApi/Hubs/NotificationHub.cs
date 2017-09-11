using Microsoft.AspNet.SignalR;
using Social.Infrastructure;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Social.WebApi.Hubs
{
    public class NotificationHub : Hub
    {
        public NotificationHub()
        {
        }

        public override Task OnConnected()
        {
            var connectionManager = GetConnectionManager();
            int siteId = GetSiteId();
            connectionManager.Connect(siteId, 0, Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionManager = GetConnectionManager();
            int siteId = GetSiteId();
            connectionManager.Disconnect(siteId, 0, Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var connectionManager = GetConnectionManager();
            int siteId = GetSiteId();
            connectionManager.Reconnect(siteId, 0, Context.ConnectionId);
            return base.OnReconnected();
        }

        private int GetSiteId()
        {
            return int.Parse(Context.QueryString["siteId"]);
        }

        public INotificationConnectionManager GetConnectionManager()
        {
            return GlobalHost.DependencyResolver.GetService(typeof(INotificationConnectionManager)) as INotificationConnectionManager;
        }
    }
}