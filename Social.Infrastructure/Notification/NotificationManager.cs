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
        public async Task NotifyNewConversation(int siteId, int conversationId)
        {
            HttpClient client = CreateHttpClient();
            await client.GetAsync($"/api/notifications/conversation-created?siteId={siteId}&conversationId={conversationId}");
        }

        public async Task NotifyUpdateConversation(int siteId, int conversationId)
        {
            HttpClient client = CreateHttpClient();
            await client.GetAsync($"/api/notifications/conversation-updated?siteId={siteId}&conversationId={conversationId}");
        }

        public async Task NotifyNewFacebookComment(int siteId, int messageId)
        {
            HttpClient client = CreateHttpClient();
            await client.GetAsync($"/api/notifications/facebook-comment-created?siteId={siteId}&messageId={messageId}");
        }

        public async Task NotifyNewFacebookMessage(int siteId, int messageId)
        {
            HttpClient client = CreateHttpClient();
            await client.GetAsync($"/api/notifications/facebook-message-created?siteId={siteId}&messageId={messageId}");
        }

        public async Task NotifyNewTwitterTweet(int siteId, int messageId)
        {
            HttpClient client = CreateHttpClient();
            await client.GetAsync($"/api/notifications/twitter-tweet-created?siteId={siteId}&messageId={messageId}");
        }

        public async Task NotifyNewTwitterDirectMessage(int siteId, int messageId)
        {
            HttpClient client = CreateHttpClient();
            await client.GetAsync($"/api/notifications/twitter-direct-message-created?siteId={siteId}&messageId={messageId}");
        }

        private HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AppSettings.NotificationApiBaseAddress);
            return client;
        }
    }
}
