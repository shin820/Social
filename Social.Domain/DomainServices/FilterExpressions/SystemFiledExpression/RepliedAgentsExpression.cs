using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{ 
    public class RepliedAgentsExpression : OptionExpression
    {
        public RepliedAgentsExpression():base("Replied Agents", "")
        {
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            int value = int.Parse(condition.Value);
            return  t => t.Messages.Any(m => m.SendAgentId == value);
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            int value = int.Parse(condition.Value);
            return t => t.Messages.Any(m => m.SendAgentId != value);
        }

        protected override object GetValue(string value)
        {
            return null;
        }
    }
}
