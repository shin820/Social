using Framework.Core;
using System.Threading.Tasks;

namespace Social.Infrastructure
{
    public interface INotificationManager : ITransient
    {
        Task NotifyNewConversation(int siteId, int conversationId);
        Task NotifyNewFacebookComment(int siteId, int messageId);
        Task NotifyNewFacebookMessage(int siteId, int messageId);
        Task NotifyNewTwitterDirectMessage(int siteId, int messageId);
        Task NotifyNewTwitterTweet(int siteId, int messageId);
        Task NotifyUpdateConversation(int siteId, int conversationId);
        Task NotifyNewPublicFilter(int siteId, int filterId);
        Task NotifyDeletePublicFilter(int siteId, int filterId);
        Task NotifyUpdatePublicFilter(int siteId, int filterId);
    }
}