using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.FilterExpressions
{
    public abstract class StringExpression : IConditionExpression
    {
        private string _propertyName;

        public StringExpression(string propertyName)
        {
            _propertyName = propertyName;
        }

        public virtual bool IsMatch(FilterCondition condition)
        {
            return condition.Field.DataType == FieldDataType.String && condition.Field.Name == _propertyName;
        }

        public virtual Expression<Func<Conversation, bool>> Build(FilterCondition condition)
        {
            if (condition.MatchType == ConditionMatchType.Is)
            {
                return Is(condition);
            }
            if (condition.MatchType == ConditionMatchType.IsNot)
            {
                return IsNot(condition);
            }
            if (condition.MatchType == ConditionMatchType.Contain)
            {
                return Contain(condition);
            }
            if (condition.MatchType == ConditionMatchType.NotContain)
            {
                return NotContain(condition);
            }
            if (condition.MatchType == ConditionMatchType.LogicalExpression)
            {
                return LogicalExpression(condition);
            }
            
            return null;
        }

        protected abstract Expression<Func<Conversation, bool>> Is(FilterCondition condition);
        protected abstract Expression<Func<Conversation, bool>> IsNot(FilterCondition condition);
        protected abstract Expression<Func<Conversation, bool>> Contain(FilterCondition condition);
        protected abstract Expression<Func<Conversation, bool>> NotContain(FilterCondition condition);
        protected abstract Expression<Func<Conversation, bool>> LogicalExpression(FilterCondition condition);
    }
}
