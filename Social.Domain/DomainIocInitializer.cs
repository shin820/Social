using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Framework.Core;
using Framework.Core.EntityFramework;
using Framework.Core.UnitOfWork;
using Social.Domain.DomainServices;
using System.Data.Entity;

namespace Social.Domain
{
    public class DomainIocInitializer
    {
        public static void Init(IKernel kernel)
        {
            kernel.Register(
                Component.For(typeof(IRepository<>))
                .ImplementedBy(typeof(SiteDataRepository<>))
                .LifestylePerWebRequest()
            );
        }
    }
}
