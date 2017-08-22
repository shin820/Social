using Framework.Core;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain
{
    public interface IFilterService : IDomainService<Filter>
    {
        void UpdateFilter(Filter filter, FilterCondition[] contiditons);
        void DeleteConditons(Filter updateFilter);
        int GetConversationNum(Filter filter);
        string GetCreatedByName(Filter filter);
        void CheckFieldIdExist(List<FilterCondition> filterConditons);
        void CheckFieldValue(List<FilterCondition> filterConditons);
    }
    public class FilterService : DomainService<Filter>, IFilterService
    {
        private IRepository<Filter> _filterRepo;
        private IRepository<FilterCondition> _filterConditionRepo;
        private IRepository<ConversationField> _conversationFieldRepo;       
        private IRepository<Conversation> _conversationService;
        private IRepository<SocialUser> _userRepo;
        private IConversationFieldService _conversationFieldOptionService;
        private IConversationService _conversation;

        public FilterService(
            IRepository<FilterCondition> filterConditionRepo,
            IRepository<Filter> filterRepo,
            IRepository<Conversation> conversationService,
            IRepository<SocialUser> userRepo, IConversationService conversation,
            IRepository<ConversationField> conversationFieldRepo,
            IConversationFieldService conversationFieldOptionService)
        {
            _filterConditionRepo = filterConditionRepo;
            _filterRepo = filterRepo;
            _conversationService = conversationService;
            _conversation = conversation;
            _userRepo = userRepo;
            _conversationFieldRepo = conversationFieldRepo;
            _conversationFieldOptionService = conversationFieldOptionService;
        }

        public void DeleteConditons(Filter updateFilter)
        {
            if (updateFilter.Conditions.Count() > 0)
            {
                List<FilterCondition> filterConditons = new List<FilterCondition>();
                filterConditons = updateFilter.Conditions.ToList();
                _filterConditionRepo.DeleteMany(filterConditons.ToArray());
            }
        }

        public void UpdateFilter(Filter filter, FilterCondition[] contiditons)
        {
            CheckFieldIdExist(contiditons.ToList());
            CheckFieldValue(contiditons.ToList());
            foreach (var condition in contiditons)
            {
                _filterConditionRepo.Insert(filter.Conditions[0]);
            }
            _filterRepo.Update(filter);

        }

        public int GetConversationNum(Filter filter)
        {
            return _conversation.FindAll(filter).Count();
        }

        public string GetCreatedByName(Filter filter)
        {
            if (_userRepo.Find(filter.CreatedBy) != null)
            {
                return _userRepo.Find(filter.CreatedBy).Name;
            }
            else
                return null;
        }

        public void CheckFieldIdExist(List<FilterCondition> filterConditons)
        {
            List<int> fieldIds = new List<int>();

            foreach (var filterCondition in filterConditons)
            {
                fieldIds.Add(filterCondition.FieldId);
            }
            fieldIds.RemoveAll(a => _conversationFieldRepo.FindAll().Where(t => fieldIds.Contains(t.Id)).Select(t => t.Id).ToList().Contains(a));
            if (fieldIds.Count != 0)
            {
                throw SocialExceptions.BadRequest($"FieldId '{fieldIds[0]}' not exists");
            }
            
        }

        public void CheckFieldValue(List<FilterCondition> filterConditons)
        {
            var conversationField = _conversationFieldOptionService.FindAllAndFillOptions().Where(t=> filterConditons.Select(f => f.FieldId).Contains(t.Id)).ToList();
            foreach (var filterConditon in filterConditons)
            {
                if (conversationField.Where(t => t.Id == filterConditon.FieldId).FirstOrDefault().DataType == FieldDataType.DateTime)
                {
                    if (conversationField.Where(t => t.Id == filterConditon.FieldId && t.Options.Any(o => o.Value == filterConditon.Value)).Count() == 0)
                    {
                        if (filterConditon.MatchType == ConditionMatchType.Between)
                        {
                            string[] value = filterConditon.Value.Split('|');
                            if (conversationField.Where(t => t.Id == filterConditon.FieldId && t.Options.Any(o => o.Value == value[0])).Count() == 0)
                            {
                                DateTime DateTime1;
                                if (!DateTime.TryParse(value[0], out DateTime1))
                                {
                                    throw SocialExceptions.BadRequest("The value of date time is invalid");
                                }
                            }
                            if (conversationField.Where(t => t.Id == filterConditon.FieldId && t.Options.Any(o => o.Value == value[1])).Count() == 0)
                            {
                                DateTime DateTime2;
                                if (!DateTime.TryParse(value[1], out DateTime2))
                                {
                                    throw SocialExceptions.BadRequest("The value of date time is invalid");
                                }
                            }
                        }
                        else
                        {
                            DateTime date;
                            if (!DateTime.TryParse(filterConditon.Value, out date))
                            {
                                throw SocialExceptions.BadRequest($"The value's type is not DateTime : '{filterConditon.Value}' ");
                            }
                        }
                    }
                }
                else if (conversationField.Where(t => t.Id == filterConditon.FieldId).FirstOrDefault().DataType == FieldDataType.Number)
                {
                    int number;
                    if (!int.TryParse(filterConditon.Value, out number))
                    {
                        throw SocialExceptions.BadRequest($"The value's type is not Number : '{filterConditon.Value}' ");
                    }
                }
                else if (conversationField.Where(t => t.Id == filterConditon.FieldId).FirstOrDefault().DataType == FieldDataType.Option)
                {
                    //   var conversationFieldOption = _conversationFieldOptionService.FindAll().Where(t => t.Value == filterConditon.Value && t.FieldId == filterConditon.FieldId).ToList();

                    if (conversationField.Where(t => t.Id == filterConditon.FieldId && t.Options.Any(o => o.Value == filterConditon.Value)).Count() == 0)
                    {
                        throw SocialExceptions.BadRequest($"The value's type is not Option : '{filterConditon.Value}' ");
                    }
                }
            }
        }

        }
}
