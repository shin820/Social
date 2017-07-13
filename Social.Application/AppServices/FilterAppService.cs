using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{

    public interface IFilterAppService
    {
        List<FilterDto> FindAll();
        FilterDto Find(int id);
        FilterDto Insert(FilterCreateDto createDto);
        void Delete(int id);
        void Update(FilterUpdateDto updateDto);
    }


    public class FilterAppService : AppService, IFilterAppService
    {
        private IDomainService<Filter> _domainService;
        private IDomainService<FilterCondition> _domainForConditonService;
       // private IFilterService _FilterService;

        public FilterAppService(IDomainService<Filter> domainService, IDomainService<FilterCondition> domainForConditonService)
        {
            _domainService = domainService;
            _domainForConditonService = domainForConditonService;
        }

        public List<FilterDto> FindAll()
        {
            return _domainService.FindAll().Where(u =>u.IfPublic == true || u.CreatedBy == UserContext.UserId).ProjectTo<FilterDto>().ToList();
        }

        public FilterDto Find(int id)
        {
            var filter = _domainService.Find(id);
            return Mapper.Map<FilterDto>(filter);
        }

        public FilterDto Insert(FilterCreateDto createDto)
        {
            var filter = Mapper.Map<Filter>(createDto);

            List<FilterCondition> filterConditons = new List<FilterCondition>();
            filterConditons = Mapper.Map<List<FilterConditionCreateDto>, List<FilterCondition>>(createDto.Conditions.ToList());

            filter.Conditions = filterConditons;
            filter = _domainService.Insert(filter);

            return Mapper.Map<FilterDto>(filter);
        }

        public void Delete(int id)
        {
            _domainService.Delete(id);
        }

        public void Update(FilterUpdateDto updateDto)
        {
            // old filter = get old filer
            // dto->old filter
            // update filter

            // delete old filter ' s condition
            // add conditino

            FilterService filterService = new FilterService();
            
            var updateFilter = _domainService.Find(updateDto.Id);
            //List<int> ids = new List<int>();
            //foreach (var conditon in updateFilter.Conditions)
            //    ids.Add(conditon.Id);
            //foreach(int id in ids)
            //    _domainForConditonService.Delete(id);
            filterService.DeleteConditons(updateFilter);
            Mapper.Map(updateDto, updateFilter);

            if (updateFilter.Conditions.Count() != 0)
            {
                    _domainForConditonService.Insert(updateFilter.Conditions[0]);                
            }
            _domainService.Update(updateFilter);
            //var filter = Mapper.Map<Filter>(updateDto);

        }

    }
}
