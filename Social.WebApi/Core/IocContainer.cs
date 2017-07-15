using Castle.Windsor;
using Framework.WebApi;
using Social.Application;
using System.Reflection;
using System.Web.Http;

namespace Social.WebApi.Core
{
    public static class IocContainer
    {
        public static void Setup()
        {
            Framework.Core.DependencyResolver dependencyResolver = new Framework.Core.DependencyResolver();
            dependencyResolver.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
            new ApplicationServicesRegistrar(dependencyResolver).RegisterServices();

            dependencyResolver.Install(
                new ControllersInstaller()
                );

            GlobalConfiguration.Configuration.DependencyResolver = new CastleDependencyResolver(dependencyResolver.IocContainer.Kernel);
        }
    }
}