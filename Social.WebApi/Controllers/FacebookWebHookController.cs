using Framework.WebApi;
using Microsoft.AspNet.SignalR;
using Social.Application.AppServices;
using Social.Infrastructure.Facebook;
using Social.WebApi.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static Social.Infrastructure.Facebook.FacebookWebHookClient;

namespace Social.WebApi.Controllers
{
    [IgnoreSiteId]
    public class FacebookWebHookController : ApiController
    {
        private IFacebookAppService _facebookWebHookAppService;

        public FacebookWebHookController(IFacebookAppService facebookWebHookAppService)
        {
            _facebookWebHookAppService = facebookWebHookAppService;
        }

        private Lazy<IHubContext> _hub = new Lazy<IHubContext>(() => GlobalHost.ConnectionManager.GetHubContext<FacebookHub>());

        protected IHubContext Hub
        {
            get { return _hub.Value; }
        }

        // GET api/values/5
        public int Get()
        {
            var queryStrings = Request.GetQueryNameValuePairs();
            var challenge = queryStrings.FirstOrDefault(t => t.Key == "hub.challenge").Value;
            return int.Parse(challenge);
        }

        // POST api/values
        public async Task<IHttpActionResult> Post()
        {
            try
            {
                var request = Request;
                string rawData = await request.Content.ReadAsStringAsync();
                Hub.Clients.All.newRaw(rawData);

                FbHookData data = await request.Content.ReadAsAsync<FbHookData>();
                 await _facebookWebHookAppService.ProcessWebHookData(data);

                if (data == null || !data.Entry.Any())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Hub.Clients.All.newRaw(ex.StackTrace);
                return Ok();
            }

            return Ok();
        }
    }
}