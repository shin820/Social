using Framework.Core;
using System.Threading.Tasks;

namespace Social.Infrastructure
{
    public interface INotificationManager : ITransient
    {
        Task NotifyNewConversation<T>(int siteId, T data);
        Task NotifyNewFacebookComment<T>(int siteId, T data);
        Task NotifyNewFacebookMessage<T>(int siteId, T data);
        Task NotifyNewTwitterDirectMessage<T>(int siteId, T data);
        Task NotifyNewTwitterTweet<T>(int siteId, T data);
        Task NotifyUpdateConversation<T>(int siteId, T data);
    }
}