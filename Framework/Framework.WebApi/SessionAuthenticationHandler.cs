using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Framework.WebApi
{
    public class SessionAuthenticationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int siteId = request.GetSiteId();
            //int currentOperatorId = Comm.Comm100.Framework.Http.SessionHelper.GetSessionValue_CurrentOperatorId(siteId);
            int currentOperatorId = 1;
            if (currentOperatorId > 0)
            {
                ClaimsIdentity cliamsIdentity = new ClaimsIdentity("SessionIdentity");
                cliamsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, currentOperatorId.ToString()));

                request.GetRequestContext().Principal = new ClaimsPrincipal(cliamsIdentity);
            }

            return base.SendAsync(request, cancellationToken);

        }
    }
}
