using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure
{
    public class NotificationManager : INotificationManager
    {
        public async Task NotifyNewConversation<T>(int siteId, T data)
        {
            HttpClient client = CreateHttpClient();
            await client.PostAsJsonAsync($"/api/notifications/conversation-created?siteId={siteId}", data);
        }

        public async Task NotifyUpdateConversation<T>(int siteId, T data)
        {
            HttpClient client = CreateHttpClient();
            await client.PostAsJsonAsync($"/api/notifications/conversation-updated?siteId={siteId}", data);
        }

        public async Task NotifyNewFacebookComment<T>(int siteId, T data)
        {
            HttpClient client = CreateHttpClient();
            await client.PostAsJsonAsync("/api/notifications/facebook-comment-created?siteId={siteId}", data);
        }

        public async Task NotifyNewFacebookMessage<T>(int siteId, T data)
        {
            HttpClient client = CreateHttpClient();
            await client.PostAsJsonAsync("/api/notifications/facebook-message-created?siteId={siteId}", data);
        }

        public async Task NotifyNewTwitterTweet<T>(int siteId, T data)
        {
            HttpClient client = CreateHttpClient();
            await client.PostAsJsonAsync("/api/notifications/twitter-tweet-created?siteId={siteId}", data);
        }

        public async Task NotifyNewTwitterDirectMessage<T>(int siteId, T data)
        {
            HttpClient client = CreateHttpClient();
            await client.PostAsJsonAsync("/api/notifications/twitter-direct-message-created?siteId={siteId}", data);
        }

        private HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AppSettings.NotificationApiBaseAddress);
            return client;
        }
    }
}
