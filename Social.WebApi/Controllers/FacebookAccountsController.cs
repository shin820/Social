using Framework.Core.UnitOfWork;
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
using System.Web.Http.Description;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/facebook-accounts")]
    public class FacebookAccountsController : ApiController
    {
        private IUnitOfWorkManager _uowManager;
        private IFacebookAccountAppService _appService;

        public FacebookAccountsController(
            IUnitOfWorkManager uowManager,
            IFacebookAccountAppService appService
            )
        {
            _uowManager = uowManager;
            _appService = appService;
        }

        [HttpGet]
        [Route("integration-request")]
        public IHttpActionResult IntegrationRequest([Required]string redirectUri)
        {
            return Redirect(FbClient.GetAuthUrl(redirectUri));
        }

        [HttpGet]
        [Route("pending-add-pages")]
        public async Task<PendingAddFacebookPagesDto> GetPendingAddPages([Required]string code, [Required]string redirectUri)
        {
            return await _appService.GetPendingAddPagesAsync(code, redirectUri);
        }

        [HttpGet]
        [Route("pages")]
        public IList<FacebookPageListDto> GetPages()
        {
            return _appService.GetPages();
        }

        [HttpGet]
        [Route("pages/{id}", Name = "GetFacebookPage")]
        [ResponseType(typeof(FacebookPageDto))]
        public IHttpActionResult GetPage(int id)
        {
            var page = _appService.GetPage(id);
            if (page == null)
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            return Ok(page);
        }

        [HttpPost]
        [Route("pages")]
        public async Task<IHttpActionResult> AddPage([Required] AddFaceboookPageDto dto)
        {
            var pageDto = await _appService.AddPageAsync(dto);
            return CreatedAtRoute("GetFacebookPage", new { id = pageDto.Id }, pageDto);
        }

        [HttpPut]
        [Route("pages/{id}")]
        public FacebookPageDto UpdatePage(int id, [Required]UpdateFacebookPageDto dto)
        {
            var page = _appService.UpdatePage(id, dto);
            return page;
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
