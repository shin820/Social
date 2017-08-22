using Framework.WebApi;
using Microsoft.AspNet.SignalR;
using Social.Application.AppServices;
using Social.Infrastructure;
using Social.Infrastructure.Facebook;
using Social.WebApi.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using static Social.Infrastructure.Facebook.FacebookWebHookClient;

namespace Social.WebApi.Controllers
{
    /// <summary>
    /// api used by facebook.
    /// </summary>
    [IgnoreSiteId]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class FacebookWebHookController : ApiController
    {
        private IFacebookAppService _facebookWebHookAppService;

        /// <summary>
        /// FacebookWebHookController
        /// </summary>
        /// <param name="facebookWebHookAppService"></param>
        public FacebookWebHookController(IFacebookAppService facebookWebHookAppService)
        {
            _facebookWebHookAppService = facebookWebHookAppService;
        }

        private Lazy<IHubContext> _hub = new Lazy<IHubContext>(() => GlobalHost.ConnectionManager.GetHubContext<FacebookHub>());

        protected IHubContext Hub
        {
            get { return _hub.Value; }
        }

        /// <summary>
        /// This api is used by facebook when setting call back url at facebook.
        /// </summary>
        /// <returns></returns>
        public int Get()
        {
            var queryStrings = Request.GetQueryNameValuePairs();
            var challenge = queryStrings.FirstOrDefault(t => t.Key == "hub.challenge").Value;
            return int.Parse(challenge);
        }

        /// <summary>
        /// This is call back url for facebook web hook.
        /// </summary>
        /// <returns></returns>
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
                Logger.Error(ex);
                Hub.Clients.All.newRaw(ex.StackTrace);
                return Ok();
            }

            return Ok();
        }
    }
}