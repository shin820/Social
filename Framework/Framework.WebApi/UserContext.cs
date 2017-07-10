using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Framework.WebApi
{
    public class UserContext : IUserContext
    {
        private int _userId;
        public UserContext()
        {
            var userIdOrNull = Thread.CurrentPrincipal.Identity.GetUserId();
            if (userIdOrNull != null)
            {
                this._userId = userIdOrNull.Value;
            }
        }

        public int UserId
        {
            get { return _userId; }
        }

        public int? SiteId
        {
            get
            {
                string siteIdStr = string.Empty;
                try
                {
                    siteIdStr = HttpContext.Current.Request["siteId"];
                }
                catch (HttpRequestValidationException ex)
                {
                    //todo 
                    //log
                    return 0;
                }
                int siteId;
                return int.TryParse(siteIdStr, out siteId) ? (int?)siteId : null;
            }
        }
    }
}
