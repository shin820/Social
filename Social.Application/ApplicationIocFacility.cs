using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Framework.Core;
using Social.Domain;

namespace Social.Application
{
    public class ApplicationIocFacility : AbstractFacility
    {
        protected override void Init()
        {
            Kernel.Register(
                Classes.FromAssemblyInThisApplication()
                       .BasedOn(typeof(AppService))
                       .WithServiceAllInterfaces()
                       .LifestylePerWebRequest()
            );

            DomainIocInitializer.Init(Kernel);
        }
    }
}
