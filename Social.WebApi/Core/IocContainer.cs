using Castle.Windsor;
using Framework.WebApi;
using System.Web.Http;

namespace Social.WebApi.Core
{
    public static class IocContainer
    {
        public static void Setup()
        {
            Framework.Core.DependencyResolver dependencyResolver = new Framework.Core.DependencyResolver();
            dependencyResolver.Install(
                new ControllersInstaller(),
                new KBInstaller()
                );

            GlobalConfiguration.Configuration.DependencyResolver = new CastleDependencyResolver(dependencyResolver.IocContainer.Kernel);
        }
    }
}