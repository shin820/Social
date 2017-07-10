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

namespace Social.IntegrationTest
{
    public class IntegrationTestInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

              Component.For<IUserContext>().ImplementedBy<NullUserContext>().LifestyleTransient(),

              //Component.For<IFacebookService>()
              //  .ImplementedBy<FacebookService>()
              //  .LifestyleTransient(),

              //Classes.FromAssemblyInThisApplication()
              //.BasedOn<IConversationSrategy>()
              //.WithServiceSelf()
              //.WithServiceAllInterfaces()
              //.LifestylePerThread(),
              Classes.FromAssemblyInThisApplication()
              .BasedOn<ITransient>()
              .WithServiceAllInterfaces()
              .WithServiceSelf()
              .LifestyleTransient(),


                Component.For(typeof(IDomainService<>))
                .ImplementedBy(typeof(DomainService<>))
                .LifestylePerThread(),

                Classes.FromAssemblyInThisApplication()
                .BasedOn(typeof(DomainService<>))
                .WithServiceAllInterfaces()
                .LifestylePerThread(),

                Component.For(typeof(DbContext))
                .UsingFactoryMethod(k => { return TestDbContextFactory.Create(k); })
                .LifestylePerThread(),

                Component.For(typeof(IRepository<>))
                .ImplementedBy(typeof(EFRepository<>))
                .LifestylePerThread(),

                Classes.FromAssemblyInThisApplication()
                .BasedOn(typeof(EFRepository<>))
                .WithServiceAllInterfaces()
                .LifestylePerThread()
           );
        }

        public class TestDbContextFactory
        {
            public static DbContext Create(IKernel kernel)
            {
                var connectionString = ConfigurationManager.ConnectionStrings["SiteDataContext"].ConnectionString;
                var userContext = kernel.Resolve<IUserContext>();
                return new SiteDataContext(connectionString, userContext);
            }
        }
    }
}
