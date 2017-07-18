using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Infrastructure;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/social_accounts")]
    public class SocialAccountsController : ApiController
    {
        private ISocialAccountAppService _appService;

        public SocialAccountsController(ISocialAccountAppService appService)
        {
            _appService = appService;
        }

        [HttpGet]
        [Route("facebook-integration-request")]
        public IHttpActionResult FacebookIntegrationRequest()
        {
            return Redirect(FbClient.GetAuthUrl(AppSettings.FacebookRedirectUri));
        }

        [HttpGet]
        [Route("facebook-pages")]
        public async Task<FacebookPagesToBeAddDto> FacebookPages([Required]string code)
        {
            return await _appService.GetFacebookPages(code, AppSettings.FacebookRedirectUri);
        }
    }
}
