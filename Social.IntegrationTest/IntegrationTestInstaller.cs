using Castle.MicroKernel.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Framework.Core;
using Social.Domain.DomainServices;
using System.Data.Entity;
using Social.Domain;
using Framework.EntityFramework;
using Castle.MicroKernel;
using System.Configuration;
using Social.Domain.DomainServices.Facebook;
using Framework.Core.UnitOfWork;

namespace Social.IntegrationTest
{
    public class IntegrationTestInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

              Component.For<IUserContext>().ImplementedBy<NullUserContext>().LifestyleTransient(),

              Classes.FromAssemblyInThisApplication()
              .BasedOn<ITransient>()
              .WithServiceAllInterfaces()
              .WithServiceSelf()
              .LifestyleTransient(),

              Classes.FromAssemblyInThisApplication()
                       .BasedOn(typeof(AppService))
                       .WithServiceAllInterfaces()
                       .LifestyleTransient(),

              Component.For(typeof(IDomainService<>))
              .ImplementedBy(typeof(DomainService<>))
              .LifestyleTransient(),

                Classes.FromAssemblyInThisApplication()
                .BasedOn(typeof(DomainService<>))
                .WithServiceAllInterfaces()
                .LifestyleTransient(),

                Component.For(typeof(IRepository<>))
                .ImplementedBy(typeof(SiteDataRepository<>))
                .LifestyleTransient(),

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

        public class TestDbContextFactory
        {
            //public static DbContext Create(IKernel kernel)
            //{
            //    var connectionString = ConfigurationManager.ConnectionStrings["SiteDataContext"].ConnectionString;
            //    var userContext = kernel.Resolve<IUserContext>();
            //    return new SiteDataContext(connectionString, userContext);
            //}
        }
    }
}
