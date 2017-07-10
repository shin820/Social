using Castle.MicroKernel;
using System.Data.Entity;
using System.Threading;
using System.Configuration;
using System.Security.Claims;
using Social.Infrastructure;
using Framework.Core;

namespace Social.Domain
{
    public class DbContextFactory
    {
        public static DbContext Create(IKernel kernel)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SiteDataContext"].ConnectionString;
            var userContext = kernel.Resolve<IUserContext>();
            if (userContext != null && userContext.SiteId.HasValue)
            {
                // returne new SiteDataContext(DbConfigure.GetConnectionStringForSiteDatabase(userContext.SiteId.Value),userContext);
                return new SiteDataContext(connectionString, userContext);
            }

            return new DbContext("error");
        }
    }
}
