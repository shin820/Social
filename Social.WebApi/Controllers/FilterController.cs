using Framework.Core;
using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/filters")]
    public class FilterController : ApiController
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

        [Route("manage-filters")]
        public List<FilterManageDto> GetManegeFilters()
        {
            return _appService.FindManageFilters();
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
            var filter = _appService.Insert(createDto);

            return CreatedAtRoute("GetFilter", new { id = filter.Id }, filter);
        }

        [Route("{id}", Name = "PutFilter")]
        public FilterDetailsDto PutFilter(int id, FilterUpdateDto createDto)
        {
            createDto = createDto ?? new FilterUpdateDto();
            return _appService.Update(id, createDto);
        }

        [Route("{id}")]
        public int DeleteFilter(int id)
        {
            _appService.Delete(id);
            return id;
        }

        [HttpPost]
        [Route("sorting")]
        public IList<FilterManageDto> SortingFilters([Required]IList<FilterSortDto> sortDtoList)
        {
            if (!sortDtoList.Any())
            {
                throw SocialExceptions.BadRequest("sortDtoList is required.");
            }

            return _appService.Sorting(sortDtoList);
        }
    }
}