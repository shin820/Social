using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure
{
    public static class AppSettings
    {
        public static string TwitterConsumerKey
        {
            get
            {
                return GetAppSetting("TwitterConsumerKey");
            }
        }

        public static string TwitterConsumerSecret
        {
            get
            {
                return GetAppSetting("TwitterConsumerSecret");
            }
        }

        public static string FacebookClientId
        {
            get
            {
                return GetAppSetting("FacebookClientId");
            }
        }

        public static string FacebookClientSecret
        {
            get
            {
                return GetAppSetting("FacebookClientSecret");
            }
        }

        public static string FacebookRedirectUri
        {
            get
            {
                return GetAppSetting("FacebookOAuthRedirectUri");
            }
        }

        public static string TwitterPullDirectMessagesJobCronExpression
        {
            get
            {
                return GetAppSetting("TwitterPullDirectMessagesJob_CronExpression");
            }
        }

        public static string TwitterPullTweetsJobCronExpression
        {
            get
            {
                return GetAppSetting("TwitterPullTweetsJob_CronExpression");
            }
        }

        public static string FacebookPullTaggedVisitorPostsJobCronExpression
        {
            get
            {
                return GetAppSetting("FacebookPullTaggedVisitorPostsJob_CronExpression");
            }
        }

        public static string FacebookPullVisitorPostsFromFeedJobCronExpression
        {
            get
            {
                return GetAppSetting("FacebookPullVisitorPostsFromFeedJob_CronExpression");
            }
        }

        public static string FacebookPullMessagesJobCronExpression
        {
            get
            {
                return GetAppSetting("FacebookPullMessagesJob_CronExpression");
            }
        }

        public static string SocialJobWindowsServiceName
        {
            get
            {
                return GetAppSetting("SocialJobWindowsServiceName");
            }
        }

        public static string SocialJobWindowsServiceDisplayName
        {
            get
            {
                return GetAppSetting("SocialJobWindowsServiceDisplayName");
            }
        }

        public static string SocialJobWindowsServiceDescription
        {
            get
            {
                return GetAppSetting("SocialJobWindowsServiceDescription");
            }
        }

        private static string GetAppSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException($"Invalid configuration for '{key}'.");
            }
            return value;
        }
    }
}
