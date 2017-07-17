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
    }
}
