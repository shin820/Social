using Framework.Core;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
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
    }
    public class FilterService : DomainService<Filter>, IFilterService
    {
        private IRepository<Filter> _filterRepo;
        private IRepository<FilterCondition> _filterConditionRepo;
        private IRepository<Conversation> _ConversationService;
        private IRepository<SocialUser> _UserRepo;
        private IConversationService _Conversation;

        public FilterService(
            IRepository<FilterCondition> filterConditionRepo,
            IRepository<Filter> filterRepo,
            IRepository<Conversation> ConversationService,
            IRepository<SocialUser> UserRepo, IConversationService Conversation)
        {
            _filterConditionRepo = filterConditionRepo;
            _filterRepo = filterRepo;
            _ConversationService = ConversationService;
            _Conversation = Conversation;
            _UserRepo = UserRepo;
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
            foreach (var condition in contiditons)
            {
                _filterConditionRepo.Insert(filter.Conditions[0]);
            }
            _filterRepo.Update(filter);

        }

        public int GetConversationNum(Filter filter)
        {
            return _Conversation.FindAll(filter).Count();
        }

        public string GetCreatedByName(Filter filter)
        {
            return _UserRepo.Find(filter.CreatedBy).Name;
        }
    }
}
