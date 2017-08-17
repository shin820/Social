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
    public class CreatedExpression : DateTimeExpression
    {
        public CreatedExpression() : base("Created")
        {
        }

        protected override Expression<Func<Conversation, bool>> After(string date)
        {
            DateTime value = DateTime.Parse(date);
            return t => t.CreatedTime > value;
        }

        protected override Expression<Func<Conversation, bool>> Before(string date)
        {
            DateTime value = DateTime.Parse(date);
            return t => t.CreatedTime < value;
        }

        protected override Expression<Func<Conversation, bool>> Between(string date)
        {
            string[] value = date.Split('|');
            DateTime DateTime1 = DateTime.Parse(value[0]);
            DateTime DateTime2 = DateTime.Parse(value[1]);
            return t => t.CreatedTime >= DateTime1 && t.CreatedTime <= DateTime2;
        }

        protected override Expression<Func<Conversation, bool>> Is(string date)
        {
            DateTime value = DateTime.Parse(date);
            return t => t.CreatedTime.Year == value.Year
                       && t.CreatedTime.Month == value.Month
                       && t.CreatedTime.Day == value.Day;
        }
    }
}
