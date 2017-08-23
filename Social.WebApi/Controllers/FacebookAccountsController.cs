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
    /// <summary>
    /// Api used for adding facebook account to our system and managing facebook accounts in our system..
    /// </summary>
    [RoutePrefix("api/facebook-accounts")]
    public class FacebookAccountsController : ApiController
    {
        private IUnitOfWorkManager _uowManager;
        private IFacebookAccountAppService _appService;
        private IFbClient _fbClient;

        /// <summary>
        /// FacebookAccountsController
        /// </summary>
        /// <param name="uowManager"></param>
        /// <param name="appService"></param>
        /// <param name="fbClient"></param>
        public FacebookAccountsController(
            IUnitOfWorkManager uowManager,
            IFacebookAccountAppService appService,
            IFbClient fbClient
            )
        {
            _uowManager = uowManager;
            _appService = appService;
            _fbClient = fbClient;
        }

        /// <summary>
        /// Make a integration request, fron-end page should redirect to this api rather than calling this api directly.
        /// </summary>
        /// <param name="redirectUri">After social user agree to integrated to our system, the page will redirect to this url.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("integration-request")]
        public IHttpActionResult IntegrationRequest([Required]string redirectUri)
        {
            return Redirect(_fbClient.GetAuthUrl(redirectUri));
        }

        /// <summary>
        /// Get user's facebook pages, we use this api to decide which page can be integrated into our system.
        /// </summary>
        /// <param name="code">A code which provided by facebook.</param>
        /// <param name="redirectUri">This url must be equal to the redirectUri you used when making integration request.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("pending-add-pages")]
        public async Task<PendingAddFacebookPagesDto> GetPendingAddPages([Required]string code, [Required]string redirectUri)
        {
            return await _appService.GetPendingAddPagesAsync(code, redirectUri);
        }

        /// <summary>
        /// Get facebook pages in our system.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("pages")]
        public IList<FacebookPageListDto> GetPages()
        {
            return _appService.GetPages();
        }

        /// <summary>
        /// Get facebook pages by id.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Add facebook page.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("pages")]
        public async Task<IHttpActionResult> AddPage([Required] AddFaceboookPageDto dto)
        {
            var pageDto = await _appService.AddPageAsync(dto);
            return CreatedAtRoute("GetFacebookPage", new { id = pageDto.Id }, pageDto);
        }

        /// <summary>
        /// Update facebook page.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("pages/{id}")]
        public FacebookPageDto UpdatePage(int id, [Required]UpdateFacebookPageDto dto)
        {
            var page = _appService.UpdatePage(id, dto);
            return page;
        }

        /// <summary>
        /// Delete facebook page.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("pages/{id}")]
        public async Task<int> DeletePage(int id)
        {
            await _appService.DeletePageAsync(id);
            return id;
        }
    }
}
