using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{

    public interface IFilterAppService
    {
        List<FilterDto> FindAll();
        FilterDetailsDto Find(int id);
        FilterDetailsDto Insert(FilterCreateDto createDto);
        void Delete(int id);
        void Update(int id,FilterUpdateDto updateDto);
    }


    public class FilterAppService : AppService, IFilterAppService
    {
        private IFilterService _domainService;

        public FilterAppService(IFilterService domainService)
        {
            _domainService = domainService;
        }

        public List<FilterDto> FindAll()
        {
            List<Filter> Filters = _domainService.FindAll().Include(t => t.Conditions).Where(u => u.IfPublic == true || u.CreatedBy == UserContext.UserId).ToList();
            List<FilterDto> FilterDtos = new List<FilterDto>();
            foreach (var Filter in Filters)
            {
                var FilterDto = Mapper.Map<FilterDto>(Filter);
                FilterDto.ConversationNum = _domainService.GetConversationNum(Filter);
                FilterDtos.Add(FilterDto);

            }
            return FilterDtos;
        }

        public FilterDetailsDto Find(int id)
        {
            var filter = _domainService.Find(id);
            var filterDto = Mapper.Map<FilterDetailsDto>(filter);

            return filterDto;
        }

        public FilterDetailsDto Insert(FilterCreateDto createDto)
        {
            var filter = Mapper.Map<Filter>(createDto);

            List<FilterCondition> filterConditons = new List<FilterCondition>();
            filterConditons = Mapper.Map<List<FilterConditionCreateDto>, List<FilterCondition>>(createDto.Conditions.ToList());

            filter.Conditions = filterConditons;
            filter = _domainService.Insert(filter);
            CurrentUnitOfWork.SaveChanges();

            return Mapper.Map<FilterDetailsDto>(filter);
        }

        public void Delete(int id)
        {
            _domainService.Delete(id);
        }

        public void Update(int id,FilterUpdateDto updateDto)
        {
            var updateFilter = _domainService.Find(id);

            _domainService.DeleteConditons(updateFilter);
            Mapper.Map(updateDto, updateFilter);
            _domainService.UpdateFilter(updateFilter, Mapper.Map<List<FilterConditionCreateDto>, List<FilterCondition>>(updateDto.Conditions.ToList()).ToArray());
        }
    }
}
