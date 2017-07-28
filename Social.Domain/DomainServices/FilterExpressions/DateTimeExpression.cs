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
            string Date = condition.Value;
            if (condition.Value == "@Today")
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            }
            else if (condition.Value == "@Yesterday")
            {
                Date = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            }
            else if (condition.Value == "@7 Days Ago")
            {
                Date = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
            }
            else if (condition.Value == "@30 Days Ago")
            {
                Date = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            }
            if (condition.MatchType == ConditionMatchType.Is)
            {
                return Is(Date);
            }
            if (condition.MatchType == ConditionMatchType.Before)
            {
                return Before(Date);
            }
            if (condition.MatchType == ConditionMatchType.After)
            {
                return After(Date);
            }
            if (condition.MatchType == ConditionMatchType.Between)
            {
                return Between(Date);
            }

            return null;
        }

        public bool IsMatch(FilterCondition condition)
        {
            return condition.Field.DataType == FieldDataType.DateTime && condition.Field.Name == _propertyName;
        }

        protected abstract Expression<Func<Conversation, bool>> Is(string Date);
        protected abstract Expression<Func<Conversation, bool>> Before(string Date);
        protected abstract Expression<Func<Conversation, bool>> After(string Date);
        protected abstract Expression<Func<Conversation, bool>> Between(string Date);
    }
}
