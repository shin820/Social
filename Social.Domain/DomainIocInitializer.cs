using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Framework.Core;
using Framework.EntityFramework;
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
                Classes.FromAssemblyInThisApplication()
                .BasedOn<ITransient>()
                .WithServiceAllInterfaces()
                .LifestyleTransient(),

                Component.For(typeof(IDomainService<>))
                .ImplementedBy(typeof(DomainService<>))
                .LifestylePerWebRequest(),

                Classes.FromAssemblyInThisApplication()
                .BasedOn(typeof(DomainService<>))
                .WithServiceAllInterfaces()
                .LifestylePerWebRequest(),

                //Component.For(typeof(DbContext))
                //.UsingFactoryMethod(k => { return DbContextFactory.Create(k); })
                //.LifestylePerWebRequest(),

                Component.For(typeof(IRepository<>))
                .ImplementedBy(typeof(SiteDataRepository<>))
                .LifestylePerWebRequest(),

                Classes.FromAssemblyInThisApplication()
                .BasedOn(typeof(SiteDataRepository<>))
                .WithServiceAllInterfaces()
                .LifestylePerWebRequest(),

                Component.For<IUnitOfWorkManager>().ImplementedBy<UnitOfWorkManager>().LifestyleTransient(),
                Component.For<IUnitOfWork>().ImplementedBy<UnitOfWork>().LifestyleTransient(),
                Component.For<ITransactionStrategy>().ImplementedBy<TransactionStrategy>().LifestyleTransient(),
                Component.For<IDbContextResolver>().ImplementedBy<DefaultDbContextResolver>().LifestyleTransient(),
                Component.For<IConnectionStringResolver>().ImplementedBy<DefaultConnectionStringResolver>().LifestyleTransient(),
                Component.For<ICurrentUnitOfWorkProvider>().ImplementedBy<CurrentUnitOfWorkProvider>().LifestyleTransient(),

               Component.For<SiteDataContext>().ImplementedBy<SiteDataContext>()
               //.UsingFactoryMethod(k => { return DbContextFactory.Create(k); })
               .LifestyleTransient()
            );
        }
    }
}
