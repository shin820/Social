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
    public class TimeToLastMessageExpression : NumberExpression
    {
        public TimeToLastMessageExpression() : base("Time to Last Message")
        {
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            double value = double.Parse(condition.Value);
            return t => (DateTime.Now - t.LastMessageSentTime).TotalMinutes == value;
        }

        protected override Expression<Func<Conversation, bool>> IsLessThan(FilterCondition condition)
        {
            double value = double.Parse(condition.Value);
            return t => (DateTime.Now - t.LastMessageSentTime).TotalMinutes < value;
        }

        protected override Expression<Func<Conversation, bool>> IsMoreThan(FilterCondition condition)
        {
            double value = double.Parse(condition.Value);
            return t => (DateTime.Now - t.LastMessageSentTime).TotalMinutes > value;
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            double value = double.Parse(condition.Value);
            return t => (DateTime.Now - t.LastMessageSentTime).TotalMinutes != value;
        }
    }
}
