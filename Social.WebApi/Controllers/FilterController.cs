using Framework.Core;
using Social.Application.AppServices;
using Social.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("filters")]
    public class FilterController:ApiController
    {
        private IFilterAppService _appService;

        public FilterController(IFilterAppService appService)
        {
            _appService = appService;
        }

        [Route()]
        public List<FilterDto> GetFilters()
        {
            return _appService.FindAll();
        }

        [Route("{id}", Name = "GetFilter")]
        public FilterDto GetFilter(int id)
        {
            return _appService.Find(id);
        }

        [Route()]
        [ResponseType(typeof(FilterDto))]
        public IHttpActionResult PostFilter(FilterCreateDto createDto)
        {
            createDto = createDto ?? new FilterCreateDto();
            var filter = _appService.Insert(createDto);

            return CreatedAtRoute("GetFilter", new { id = filter.Id }, filter);
        }

        [Route()]
        [ResponseType(typeof(FilterUpdateDto))]
        public IHttpActionResult PutFilter(FilterUpdateDto createDto)
        {
            createDto = createDto ?? new FilterUpdateDto();
            _appService.Update(createDto);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{id}")]
        public IHttpActionResult DeleteFilter(int id)
        {
            _appService.Delete(id);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}