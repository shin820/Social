using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public class LastRepliedAgentExpression: OptionExpression
    {
        public LastRepliedAgentExpression():base("Last Replied Agent", "LastRepliedAgentId")
        {
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            int value = int.Parse(condition.Value);
            return t => t.LastRepliedAgentId.HasValue &&(int)t.LastRepliedAgentId.Value == value;
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            int value = int.Parse(condition.Value);
            return t => t.LastRepliedAgentId.HasValue && (int)t.LastRepliedAgentId.Value != value || !t.LastRepliedAgentId.HasValue;
        }

        protected override object GetValue(string rawValue)
        {
           return null;
        }
    }
}
