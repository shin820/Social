using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure
{
    public interface INotificationConnectionManager
    {
        void Connect(int siteId, int agentId, string connectionId);
        void Reconnect(int siteId, int agentId, string connectionId);
        void Disconnect(int siteId, int agentId, string connectionId);
        void RefreshCacheItem<T>(T Key, OperationType oprtType);
        IList<string> GetAllConnections();
        IList<string> GetConnectionsForConversation(int siteId, int conversationId);
    }
}
