using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure.Core
{
    public class NotificationManager : INotificationManager
    {
        private TimeSpan _delayTimeSpan = TimeSpan.Zero;

        public async Task NotifyNewConversation(int siteId, int conversationId)
        {
            await GetAsync($"/api/notifications/conversation-created?siteId={siteId}&conversationId={conversationId}");
        }

        public async Task NotifyUpdateConversation(int siteId, int conversationId, int? oldMaxLogId)
        {
            string url = $"/api/notifications/conversation-updated?siteId={siteId}&conversationId={conversationId}";
            if (oldMaxLogId != null && oldMaxLogId > 0)
            {
                url += $"&oldMaxLogId={oldMaxLogId}";
            }

            await GetAsync(url);
        }

        public async Task NotifyNewFacebookComment(int siteId, int messageId)
        {
            await GetAsync($"/api/notifications/facebook-comment-created?siteId={siteId}&messageId={messageId}");
        }

        public async Task NotifyNewFacebookMessage(int siteId, int messageId)
        {
            await GetAsync($"/api/notifications/facebook-message-created?siteId={siteId}&messageId={messageId}");
        }

        public async Task NotifyNewTwitterTweet(int siteId, int messageId)
        {
            await GetAsync($"/api/notifications/twitter-tweet-created?siteId={siteId}&messageId={messageId}");
        }

        public async Task NotifyNewTwitterDirectMessage(int siteId, int messageId)
        {
            await GetAsync($"/api/notifications/twitter-direct-message-created?siteId={siteId}&messageId={messageId}");
        }

        public async Task NotifyNewPublicFilter(int siteId, int filterId)
        {
            await GetAsync($"/api/notifications/public-filter-created?siteId={siteId}&filterId={filterId}");
        }

        public async Task NotifyDeletePublicFilter(int siteId, int filterId)
        {
            await GetAsync($"/api/notifications/public-filter-deleted?siteId={siteId}&filterId={filterId}");
        }

        public async Task NotifyUpdatePublicFilter(int siteId, int filterId)
        {
            await GetAsync($"/api/notifications/public-filter-updated?siteId={siteId}&filterId={filterId}");
        }

        public async Task GetAsync(string url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AppSettings.NotificationApiBaseAddress);
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                string msg = await response.Content.ReadAsStringAsync();
                Logger.Error(msg, null);
            }
        }

        public async Task PostAsync<T>(string url, T value)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(AppSettings.NotificationApiBaseAddress);
            var response = await client.PostAsync<T>(url, value, new JsonMediaTypeFormatter());
            if (!response.IsSuccessStatusCode)
            {
                string msg = await response.Content.ReadAsStringAsync();
                Logger.Error(msg, null);
            }
        }
    }
}
