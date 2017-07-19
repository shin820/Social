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
                string value = ConfigurationManager.AppSettings["TwitterConsumerKey"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ConfigurationErrorsException("Invalid configuration for 'TwitterConsumerKey'.");
                }
                return value;
            }
        }

        public static string TwitterConsumerSecret
        {
            get
            {
                string value = ConfigurationManager.AppSettings["TwitterConsumerSecret"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ConfigurationErrorsException("Invalid configuration for 'TwitterConsumerSecret'.");
                }
                return value;
            }
        }

        public static string FacebookClientId
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FacebookClientId"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ConfigurationErrorsException("Invalid configuration for 'FacebookClientId'.");
                }
                return value;
            }
        }

        public static string FacebookClientSecret
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FacebookClientSecret"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ConfigurationErrorsException("Invalid configuration for 'FacebookClientSecret'.");
                }
                return value;
            }
        }

        public static string FacebookRedirectUri
        {
            get
            {
                string value = ConfigurationManager.AppSettings["FacebookOAuthRedirectUri"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ConfigurationErrorsException("Invalid configuration for 'FacebookOAuthRedirectUri'.");
                }
                return value;
            }
        }
    }
}
