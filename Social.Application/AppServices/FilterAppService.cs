using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure;
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
        List<FilterListDto> FindAll();
        FilterListDto FindSummary(int id);
        List<FilterManageDto> FindManageFilters();
        FilterDetailsDto Find(int id);
        FilterDetailsDto Insert(FilterCreateDto createDto);
        void Delete(int id);
        FilterDetailsDto Update(int id, FilterUpdateDto updateDto);
        List<FilterManageDto> Sorting(IList<FilterSortDto> dtoList);
    }


    public class FilterAppService : AppService, IFilterAppService
    {
        private IFilterService _domainService;
        private IAgentService _agentService;
        private INotificationManager _notificationManager;

        public FilterAppService(
            IFilterService domainService,
            IAgentService agentService,
            INotificationManager notificationManager
            )
        {
            _domainService = domainService;
            _agentService = agentService;
            _notificationManager = notificationManager;
        }

        public List<FilterListDto> FindAll()
        {
            List<Filter> filters = _domainService.FindAll().Include(t => t.Conditions).Where(u => u.IfPublic == true || u.CreatedBy == UserContext.UserId).ToList();
            List<FilterListDto> filterDtoes = new List<FilterListDto>();
            foreach (var filter in filters)
            {
                var FilterDto = Mapper.Map<FilterListDto>(filter);
                FilterDto.ConversationNum = _domainService.GetConversationNum(filter);
                filterDtoes.Add(FilterDto);
            }
            _agentService.FillCreatedByName(filterDtoes);

            return filterDtoes;
        }

        public FilterListDto FindSummary(int id)
        {
            var filter = _domainService.Find(id);
            if (filter == null)
            {
                throw SocialExceptions.FilterNotExists(id);
            }

            var dto = Mapper.Map<FilterListDto>(filter);
            dto.ConversationNum = _domainService.GetConversationNum(filter);
            dto.CreatedByName = _agentService.GetDiaplyName(dto.CreatedBy);

            return dto;
        }

        public FilterDetailsDto Find(int id)
        {
            var filter = _domainService.Find(id);
            if (filter == null)
            {
                throw SocialExceptions.FilterNotExists(id);
            }
            var filterDto = Mapper.Map<FilterDetailsDto>(filter);
            filterDto.CreatedByName = _agentService.GetDiaplyName(filterDto.CreatedBy);
            return filterDto;
        }

        public FilterDetailsDto Insert(FilterCreateDto createDto)
        {
            var filter = Mapper.Map<Filter>(createDto);

            List<FilterCondition> filterConditons = new List<FilterCondition>();
            filterConditons = Mapper.Map<List<FilterConditionCreateDto>, List<FilterCondition>>(createDto.Conditions.ToList());
            _domainService.CheckFieldIdExist(filterConditons);
            _domainService.CheckFieldValue(filterConditons);
            filter.Conditions = filterConditons;
            filter = _domainService.Insert(filter);
            CurrentUnitOfWork.SaveChanges();

            _notificationManager.NotifyNewPublicFilter(filter.SiteId, filter.Id);

            var filterDto = Mapper.Map<FilterDetailsDto>(filter);
            List<FilterDetailsDto> filterDtos = new List<FilterDetailsDto>();
            filterDtos.Add(filterDto);
            _agentService.FillCreatedByName(filterDtos);
            return filterDto;
        }

        public void Delete(int id)
        {
            var filter = _domainService.Find(id);
            if (filter == null)
            {
                throw SocialExceptions.FilterNotExists(id);
            }
            _domainService.Delete(id);
            _notificationManager.NotifyDeletePublicFilter(filter.SiteId, filter.Id);
        }

        public FilterDetailsDto Update(int id, FilterUpdateDto updateDto)
        {
            var updateFilter = _domainService.Find(id);
            if (updateFilter == null)
            {
                throw SocialExceptions.FilterNotExists(id);
            }

            _domainService.DeleteConditons(updateFilter);
            Mapper.Map(updateDto, updateFilter);
            _domainService.UpdateFilter(updateFilter, Mapper.Map<List<FilterConditionCreateDto>, List<FilterCondition>>(updateDto.Conditions.ToList()).ToArray());
            CurrentUnitOfWork.SaveChanges();

            _notificationManager.NotifyUpdatePublicFilter(updateFilter.SiteId, updateFilter.Id);
            var filterDto = Mapper.Map<FilterDetailsDto>(updateFilter);
            List<FilterDetailsDto> filterDtos = new List<FilterDetailsDto>();
            filterDtos.Add(filterDto);
            _agentService.FillCreatedByName(filterDtos);
            return filterDto;
        }

        public List<FilterManageDto> FindManageFilters()
        {
            List<Filter> filters = _domainService.FindAll().Where(u => u.IfPublic == true || u.CreatedBy == UserContext.UserId).ToList();
            List<FilterManageDto> filterDtos = new List<FilterManageDto>();
            foreach (var filter in filters)
            {
                var filterDto = Mapper.Map<FilterManageDto>(filter);
                filterDto.CreatedByName = _agentService.GetDiaplyName(filter.CreatedBy);
                filterDtos.Add(filterDto);
            }
            return filterDtos;
        }

        public List<FilterManageDto> Sorting(IList<FilterSortDto> dtoList)
        {
            var ids = dtoList.Select(t => t.Id).ToList();
            List<Filter> filters = _domainService.FindAll().Where(t => ids.Contains(t.Id)).ToList();
            foreach (var filter in filters)
            {
                var dto = dtoList.FirstOrDefault(t => t.Id == filter.Id);
                if (dto != null)
                {
                    filter.Index = dto.Index;
                    _domainService.Update(filter);
                }
            }

            List<FilterManageDto> filterDtos = new List<FilterManageDto>();
            foreach (var filter in filters)
            {
                var filterDto = Mapper.Map<FilterManageDto>(filter);
                filterDto.CreatedByName = _agentService.GetDiaplyName(filter.CreatedBy);
                filterDtos.Add(filterDto);
            }
            return filterDtos;
        }

    }
}
