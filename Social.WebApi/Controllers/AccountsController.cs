using Social.WebApi.Models.Account;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Social.Infrastructure;
using Social.WebApi.Core;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using Framework.Core;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/accounts")]
    public class AccountsController : ApiController
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }

        static AccountsController()
        {
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();
        }

        [ApiAuthorize(Permissions = "get_me")]
        [HttpGet]
        [Route("me")]
        public Me Me()
        {
            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            var me = new Me
            {
                Id = identity.GetUserId<string>(),
                Name = identity.GetUserName(),
                Email = identity.Claims.FirstOrDefault(t => t.Type == ClaimTypes.Email).Value,
                SideId = identity.GetSideId()
            };

            return me;
        }

        //[HttpPost]
        //[Route("token")]
        //public string Authenticate(AuthenticationModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return "Invalid request!";
        //    }

        //    //todo, validate user name and secret
        //    ClaimsIdentity claimsIdentity = TestClaimsIdentity.Create();
        //    var ticket = new AuthenticationTicket(claimsIdentity, new AuthenticationProperties());
        //    ticket.Properties.IssuedUtc = DateTime.UtcNow;
        //    ticket.Properties.ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromMinutes(30));

        //    return OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
        //}
    }
}
