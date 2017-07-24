using Social.Domain.DomainServices.FilterExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using System.Linq.Expressions;

namespace Social.Domain.DomainServices
{
    class LastModifiedExpression : DateTimeExpression
    {
        public LastModifiedExpression() : base("Last Modified")
        {
        }

        protected override Expression<Func<Conversation, bool>> After(FilterCondition condition)
        {
            DateTime value = DateTime.Parse(condition.Value);
            return t => t.ModifiedTime > value;
        }

        protected override Expression<Func<Conversation, bool>> Before(FilterCondition condition)
        {
            DateTime value = DateTime.Parse(condition.Value);
            return t => t.ModifiedTime < value;
        }

        protected override Expression<Func<Conversation, bool>> Between(FilterCondition condition)
        {
            string[] value = condition.Value.Split(',');
            DateTime DateTime1 = DateTime.Parse(value[0]);
            DateTime DateTime2 = DateTime.Parse(value[1]);
            return t => t.ModifiedTime <= DateTime1 || t.ModifiedTime >= DateTime2;
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            DateTime value = DateTime.Parse(condition.Value);
            return t => ((DateTime)t.ModifiedTime).Year == value.Year
                       && ((DateTime)t.ModifiedTime).Month == value.Month
                       && ((DateTime)t.ModifiedTime).Day == value.Day;
        }
    }
}
