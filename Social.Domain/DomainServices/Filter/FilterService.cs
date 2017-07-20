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
    }
    public class FilterService : DomainService<Filter>, IFilterService
    {
        private IRepository<Filter> _filterRepo;
        private IRepository<FilterCondition> _filterConditionRepo;
        private IRepository<Conversation> _ConversationService;

        public FilterService(IRepository<FilterCondition> filterConditionRepo, IRepository<Filter> filterRepo, IRepository<Conversation> ConversationService)
        {
            _filterConditionRepo = filterConditionRepo;
            _filterRepo = filterRepo;
            _ConversationService = ConversationService;
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
            SiteDataContext db = new SiteDataContext("data source=localhost;initial catalog=Social;integrated security=True;multipleactiveresultsets=True;application name=EntityFramework");
            IQueryable<Conversation> custs = db.Conversations;

            List <object> options = new List<object>();
            List<string> fildName = new List<string>();
            List<ConditionMatchType> MatchType = new List<ConditionMatchType>();
            List<FilterType> TriggerType = new List<FilterType>();
            List<object> Useroptions = new List<object>();
            List<string> UserfildName = new List<string>();
            List<ConditionMatchType> UserMatchType = new List<ConditionMatchType>();
            List<FilterType> UserTriggerType = new List<FilterType>();

            foreach (var condition in filter.Conditions)
            {
                MatchType.Add(condition.MatchType);
                TriggerType.Add(filter.Type);
                switch (condition.FieldId)
                {
                    case 1:
                        {
                            fildName.Add("Source");
                            options.Add((ConversationSource)(int.Parse(condition.Value)));                           
                            break;
                        }
                    case 4:
                        {
                            fildName.Add("Status");
                            options.Add((ConversationStatus)(int.Parse(condition.Value))); 
                            break;
                        }
                    case 5:
                        {
                            fildName.Add("Priority");
                            options.Add((ConversationPriority)(int.Parse(condition.Value)));
                            break;
                        }
                    case 12:
                        {
                            Expression<Func<SocialUser, bool>> UserExpression = GetConditionExpression<SocialUser>(new object[]{ condition.Value }, new string[] { "Name" }, new ConditionMatchType[] { condition.MatchType },new FilterType[] { FilterType.All});
                            var userQuery = db.SocialUsers.Where(UserExpression);
                            MatchType.RemoveAt(MatchType.Count() - 1);
                            TriggerType.RemoveAt(TriggerType.Count() - 1);
                            foreach (var user in userQuery)
                            {
                                UserfildName.Add("LastMessageSenderId");
                                Useroptions.Add(user.Id);                              
                                UserMatchType.Add(ConditionMatchType.Is);
                                UserTriggerType.Add(FilterType.Any);
                            }
                            break;
                        }
                    case 16:
                        {
                            fildName.Add("LastMessageSentTime");
                            options.Add(DateTime.Parse(condition.Value));
                            break;
                        }
                    case 18:
                        {
                            fildName.Add("");
                            options.Add(DateTime.Parse(condition.Value));
                            break;
                        }
                }
            }
            options.AddRange(Useroptions);
            fildName.AddRange(UserfildName);
            MatchType.AddRange(UserMatchType);
            TriggerType.AddRange(UserTriggerType);
            var query = db.Conversations.Where(GetConditionExpression<Conversation>(options.ToArray(), fildName.ToArray(), MatchType.ToArray(), TriggerType.ToArray()));
            return query.Count();
        }

        public static Expression<Func<T, bool>> GetConditionExpression<T>(object[] options, string[] fieldName, ConditionMatchType[] MatchType, FilterType[] triggerType)
        {
            ParameterExpression left = Expression.Parameter(typeof(T), "c");
            Expression expression = Expression.Constant(false);
            
            for (int i = 0; i< options.Count();i++)
            {
                if (triggerType[i] == FilterType.All)
                {
                    expression = Expression.Constant(true);
                }
                Expression l = Expression.Property(left, typeof(T).GetProperty(fieldName[i]));
                
                Expression r = Expression.Constant(options[i]);
                Expression filters;
                if (MatchType[i] == ConditionMatchType.Is)
                {
                    filters = Expression.Equal(l, r);
                }
                else if(MatchType[i] == ConditionMatchType.IsNot)
                {
                    filters = Expression.NotEqual(l, r);
                }
                else if(MatchType[i] == ConditionMatchType.Before)
                {
                    filters = Expression.LessThan(l, r);
                }
                else if (MatchType[i] == ConditionMatchType.After)
                {
                    filters = Expression.GreaterThan(l, r);
                }
                else if(MatchType[i] == ConditionMatchType.NotContain)
                {
                    filters = Expression.Call
                       (
                        Expression.Property(left, typeof(T).GetProperty(fieldName[i])),
                        typeof(string).GetMethod("Contains", new Type[] { typeof(string) }),
                        Expression.Constant(options[i])
                        );
                    filters = Expression.IsFalse(filters);
                }
                else
                {
                     filters = Expression.Call
                        (
                         Expression.Property(left, typeof(T).GetProperty(fieldName[i])), 
                         typeof(string).GetMethod("Contains", new Type[] { typeof(string) }),
                         Expression.Constant(options[i])              
                         );
                }


                if (triggerType[i] == FilterType.Any)
                {
                    expression = Expression.Or(filters, expression);
                }
                else if(triggerType[i] == FilterType.All)
                {
                    expression = Expression.And(filters, expression);
                }
            }
            Expression<Func<T, bool>> finalExpression
                = Expression.Lambda<Func<T, bool>>(expression, new ParameterExpression[] { left });
            return finalExpression;
        }

    }
}
