using Framework.Core.UnitOfWork;
using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Tweetinvi;
using Tweetinvi.Models;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/twitter-accounts")]
    public class TwitterAccountsController : ApiController
    {
        private static IAuthenticationContext _authenticationContext;
        private IUnitOfWorkManager _uowManager;
        private ITwitterAccountAppService _appService;

        public TwitterAccountsController(
            IUnitOfWorkManager uowManager,
            ITwitterAccountAppService appService)
        {
            _uowManager = uowManager;
            _appService = appService;
        }

        /// <summary>
        /// Redirect user to go on Twitter.com to authenticate
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("integration-request")]
        public IHttpActionResult IntegrationRequest()
        {
            var appCreds = new ConsumerCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret);

            int siteId = _uowManager.Current.GetSiteId().Value;
            // Specify the url you want the user to be redirected to
            var redirectURL = $"http://localhost:20000/api/twitter-accounts/validate-twitter-auth?siteId={siteId}";
            _authenticationContext = AuthFlow.InitAuthentication(appCreds, redirectURL);
            return Redirect(_authenticationContext.AuthorizationURL);
        }

        /// <summary>
        /// Twitter will redirect it's page to this api after users agree to grant permissions to our app on Twitter.
        /// </summary>
        /// <param name="oauth_verifier"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("validate-twitter-auth")]
        public async Task<IHttpActionResult> ValidateTwitterAuth(string oauth_verifier)
        {
            var queryString = HttpContext.Current.Request.QueryString;
            var userCreds = AuthFlow.CreateCredentialsFromVerifierCode(oauth_verifier, _authenticationContext);
            await _appService.AddAccountAsync(userCreds);
            return Redirect("http://localhost:20000/Home");
        }

        [HttpGet]
        [Route("accounts")]
        public IList<TwitterAccountListDto> GetPages()
        {
            return _appService.GetAccounts();
        }

        [HttpGet]
        [Route("accounts/{id}")]
        [ResponseType(typeof(TwitterAccountDto))]
        public IHttpActionResult GetAccount(int id)
        {
            var page = _appService.GetAccount(id);
            if (page == null)
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            return Ok(page);
        }

        [HttpPut]
        [Route("accounts/{id}")]
        public TwitterAccountDto UpdateAccount(int id, [Required]UpdateTwitterAccountDto dto)
        {
            var account = _appService.UpdateAccount(id, dto);
            return account;
        }

        [HttpDelete]
        [Route("accounts/{id}")]
        public async Task<IHttpActionResult> DeleteAccount(int id)
        {
            await _appService.DeleteAccountAsync(id);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}