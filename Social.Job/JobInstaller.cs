using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Framework.Core;
using Framework.EntityFramework;
using Framework.Core.UnitOfWork;
using Social.Domain;

namespace Social.Job
{
    public class JobInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

              Component.For<IUserContext>().ImplementedBy<JobUserContext>().LifestyleTransient(),

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
                .ImplementedBy(typeof(JobRepository<>))
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

        public class JobRepository<TEntity> : UnitOfWorkEfRepository<SiteDataContext, TEntity>, IRepository<TEntity> where TEntity : Entity
        {
        }


        //public class TestDbContextFactory
        //{
        //    public static DbContext Create(IKernel kernel)
        //    {
        //        var connectionString = ConfigurationManager.ConnectionStrings["SiteDataContext"].ConnectionString;
        //        var userContext = kernel.Resolve<IUserContext>();
        //        return new SiteDataContext(connectionString, userContext);
        //    }
        //}
    }
}
