using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core
{
    public class DefaultConnectionStringResolver : IConnectionStringResolver, ITransient
    {
        public string GetNameOrConnectionStringForSiteDb(int siteId)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SiteDataContext"].ConnectionString;
            return connectionString;
        }
    }
}
