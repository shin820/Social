using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    class AgentAssigneeExpression : OptionExpression
    {
        public AgentAssigneeExpression() : base("Agent Assignee", "")
        {
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            int value = int.Parse(condition.Value);
            return t => (int)t.AgentId == value;
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            int value = int.Parse(condition.Value);
            return t => (int)t.AgentId != value;
        }

        protected override object GetValue(string rawValue)
        {
            return null;
        }
    }
}
