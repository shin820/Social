using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Domain.Entities.General;
using LinqKit;

namespace Social.Domain.DomainServices
{
    public class AgentAssigneeStatusExpression : OptionExpression
    {
        private IAgentService _AgentService;
        public AgentAssigneeStatusExpression(IAgentService AgentService) : base("Agent Assignee Status", "")
        {
            _AgentService = AgentService;
        }

        protected override object GetValue(string value)
        {
            return null;
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            int[] AgentIds = _AgentService.IsMatchStatusAgents(int.Parse(condition.Value));
            var predicate = PredicateBuilder.New<Conversation>();
            foreach (int member in AgentIds)
            {
                Expression<Func<Conversation, bool>> b = t => (int)t.AgentId == member;
                predicate = predicate.Or(b);
            }
            return predicate;
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            int[] AgentIds = _AgentService.IsMatchStatusAgents(int.Parse(condition.Value));
            var predicate = PredicateBuilder.New<Conversation>();
            foreach (int member in AgentIds)
            {
                Expression<Func<Conversation, bool>> b = t => (int)t.AgentId != member;
                predicate = predicate.And(b);
            }
            return predicate;
        }
    }
}
