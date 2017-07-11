using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;

namespace Social.Domain.DomainServices.Facebook
{
    public abstract class WebHookStrategy : IWebHookSrategy
    {
        public abstract bool IsMatch(FbHookChange change);

        public abstract Task Process(SocialAccount socialAccount, FbHookChange change);

        protected string GetSubject(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "No Subject";
            }

            return message.Length <= 200 ? message : message.Substring(200);
        }
    }
}
