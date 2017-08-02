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
    [RoutePrefix("api/filters")]
    public class FilterController:ApiController
    {
        private IFilterAppService _appService;

        public FilterController(IFilterAppService appService)
        {
            _appService = appService;
        }

        [Route()]
        public List<FilterListDto> GetFilters()
        {
            return _appService.FindAll();
        }

        [Route("{id}", Name = "GetFilter")]
        public FilterDetailsDto GetFilter(int id)
        {
            return _appService.Find(id);
        }

        [Route()]
        [ResponseType(typeof(FilterDetailsDto))]
        public IHttpActionResult PostFilter(FilterCreateDto createDto)
        {
            createDto = createDto ?? new FilterCreateDto();
            var filter =  _appService.Insert(createDto);

            return CreatedAtRoute("GetFilter", new { id = filter.Id }, filter);
        }

        [Route("{id}", Name = "PutFilter")]
        [ResponseType(typeof(FilterUpdateDto))]
        public IHttpActionResult PutFilter(int id,FilterUpdateDto createDto)
        {
            createDto = createDto ?? new FilterUpdateDto();
            _appService.Update(id,createDto);

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