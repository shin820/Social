using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Framework.Core;
using Framework.EntityFramework;
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

                Component.For(typeof(DbContext))
                .UsingFactoryMethod(k => { return DbContextFactory.Create(k); })
                .LifestylePerWebRequest(),

                Component.For(typeof(IRepository<>))
                .ImplementedBy(typeof(EFRepository<>))
                .LifestylePerWebRequest(),

                Classes.FromAssemblyInThisApplication()
                .BasedOn(typeof(EFRepository<>))
                .WithServiceAllInterfaces()
                .LifestylePerWebRequest()
            );
        }
    }
}
