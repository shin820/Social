using Social.Domain.DomainServices.FilterExpressions;
using System;
using Social.Domain.Entities;
using System.Linq.Expressions;

namespace Social.Domain.DomainServices
{
    class LastMessageSentExpression : DateTimeExpression
    {
        public LastMessageSentExpression() : base("Last Message Sent")
        {
        }

        protected override Expression<Func<Conversation, bool>> After(string date)
        {
            DateTime value = DateTime.Parse(date);
            return t => t.LastMessageSentTime > value;
        }

        protected override Expression<Func<Conversation, bool>> Before(string date)
        {
            DateTime value = DateTime.Parse(date);
            return t => t.LastMessageSentTime < value;
        }

        protected override Expression<Func<Conversation, bool>> Between(string date)
        {
            string[] value = date.Split(',');
            DateTime DateTime1 = DateTime.Parse(value[0]);
            DateTime DateTime2 = DateTime.Parse(value[1]);
            return t => t.LastMessageSentTime <= DateTime1 || t.LastMessageSentTime >= DateTime2;
        }

        protected override Expression<Func<Conversation, bool>> Is(string date)
        {
            DateTime value = DateTime.Parse(date);
            return t => t.LastMessageSentTime.Year == value.Year
                       && t.LastMessageSentTime.Month == value.Month
                       && t.LastMessageSentTime.Day == value.Day;
        }
    }
}
