using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
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
    }


    public class FilterAppService : AppService, IFilterAppService
    {
        private IDomainService<Filter> _domainService;
        private IDomainService<FilterCondition> _domainServiceFroConditon;

        public FilterAppService(IDomainService<Filter> domainService)
        {
            _domainService = domainService;

        }

        public List<FilterDto> FindAll()
        {
            return _domainService.FindAll().ProjectTo<FilterDto>().ToList();
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
            foreach (var ConditonCreateDto in createDto.Conditions)
            {
                ConditonCreateDto.FilterId = filter.Id;
                var Conditon = Mapper.Map<FilterCondition>(ConditonCreateDto);
                filterConditons.Add(Conditon);
            }

            filter.Conditions = filterConditons;
            filter = _domainService.Insert(filter);

            return Mapper.Map<FilterDto>(filter);
        }

        public void Delete(int id)
        {
            _domainService.Delete(id);
        }

    }
}
