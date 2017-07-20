using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;

namespace Social.Domain.DomainServices.FilterExpressions
{
    public abstract class DateTimeExpression : IConditionExpression
    {
        private string _propertyName;

        public DateTimeExpression(string propertyName)
        {
            _propertyName = propertyName;
        }
        public Expression<Func<Conversation, bool>> Build(FilterCondition condition)
        {
            if (condition.MatchType == ConditionMatchType.Is)
            {
                return Is(condition);
            }
            if (condition.MatchType == ConditionMatchType.Before)
            {
                return Before(condition);
            }
            if (condition.MatchType == ConditionMatchType.After)
            {
                return After(condition);
            }
            if (condition.MatchType == ConditionMatchType.Between)
            {
                return Between(condition);
            }

            return null;
        }

        public bool IsMatch(FilterCondition condition)
        {
            return condition.Field.DataType == FieldDataType.DateTime && condition.Field.Name == _propertyName;
        }

        protected abstract Expression<Func<Conversation, bool>> Is(FilterCondition condition);
        protected abstract Expression<Func<Conversation, bool>> Before(FilterCondition condition);
        protected abstract Expression<Func<Conversation, bool>> After(FilterCondition condition);
        protected abstract Expression<Func<Conversation, bool>> Between(FilterCondition condition);
    }
}
