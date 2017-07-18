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
    [RoutePrefix("api/facebook-accounts")]
    public class FacebookAccountsController : ApiController
    {
        private IFacebookAccountAppService _appService;

        public FacebookAccountsController(IFacebookAccountAppService appService)
        {
            _appService = appService;
        }

        [HttpGet]
        [Route("integration-request")]
        public IHttpActionResult IntegrationRequest()
        {
            return Redirect(FbClient.GetAuthUrl(AppSettings.FacebookRedirectUri));
        }

        [HttpGet]
        [Route("pending-add-pages")]
        public async Task<PendingAddFacebookPagesDto> GetPendingAddPages([Required]string code)
        {
            return await _appService.GetPendingAddPagesAsync(code, AppSettings.FacebookRedirectUri);
        }

        [HttpGet]
        [Route("pages")]
        public IList<FacebookPageListDto> GetPages()
        {
            return _appService.GetPages();
        }

        [HttpGet]
        [Route("pages/{id}", Name = "GetFacebookPage")]
        public async Task<IHttpActionResult> GetPage(int id)
        {
            await _appService.DeletePageAsync(id);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("pages")]
        public async Task<IHttpActionResult> AddPage([Required] AddFaceboookPageDto dto)
        {
            await _appService.AddPageAsync(dto);
            return Ok();
        }

        [HttpDelete]
        [Route("pages/{id}")]
        public async Task<IHttpActionResult> DeletePage(int id)
        {
            await _appService.DeletePageAsync(id);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
