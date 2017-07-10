using Social.WebApi.App_Start;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Social.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Microsoft.Owin.Extensions;

[assembly: OwinStartup(typeof(Startup))]

namespace Social.WebApi.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //OAuthAuthorizationServerOptions oAuthServerOptions = new OAuthAuthorizationServerOptions()
            //{
            //    AllowInsecureHttp = true,
            //    TokenEndpointPath = new PathString("/token"),
            //    AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
            //    Provider = new AuthorizationServerProvider(new TestClientStore()),
            //    RefreshTokenProvider = new RefreshTokenProvider(new TestRefreshTokenStore())
            //};

            //app.UseOAuthAuthorizationServer(oAuthServerOptions);
            //app.UseOAuthBearerAuthentication(AccountsController.OAuthBearerOptions);
            app.MapSignalR();
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            app.Use((context, next) =>
            {
                var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                httpContext.SetSessionStateBehavior(SessionStateBehavior.Required);
                return next();
            });
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}