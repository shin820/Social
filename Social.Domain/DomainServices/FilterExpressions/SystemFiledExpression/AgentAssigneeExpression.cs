using Framework.Core;
using LinqKit;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public class AgentAssigneeExpression : OptionExpression
    {
        public AgentAssigneeExpression(IUserContext UserContext, IDepartmentService DepartmentService) : base("Agent Assignee", "")
        {
            _UserContext = UserContext;
            _DepartmentService = DepartmentService;
        }
        private IUserContext _UserContext;
        private IDepartmentService _DepartmentService;
        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            int? value = default(int);
            if (condition.Value == "@My Department Member")
            {
                int[] members = _DepartmentService.GetMyDepartmentMembers( _UserContext.UserId);
                var predicate = PredicateBuilder.New<Conversation>();
                foreach (int member in members)
                {
                    Expression<Func<Conversation, bool>> b = t => (int)t.AgentId == member;
                    predicate = predicate.Or(b);                   
                }
                return predicate;
            }          
            else
            {
                value = MatchValue(condition);
            }
             
            return t => t.AgentId == value;
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            int? value = default(int);
            if (condition.Value == "@My Department Member")
            {
                int[] members = _DepartmentService.GetMyDepartmentMembers(_UserContext.UserId);
                var predicate = PredicateBuilder.New<Conversation>();
                foreach (int member in members)
                {
                    Expression<Func<Conversation, bool>> b = t => (int)t.AgentId != member;
                    predicate = predicate.And(b);
                }
                return predicate;
            }
            else
            {
                value = MatchValue(condition);
            }
            
            return t => t.AgentId != value;
        }

        protected override object GetValue(string rawValue)
        {
            return null;
        }

        private int? MatchValue(FilterCondition condition)
        {
            int? value = default(int);
            if (condition.Value == "@Me")
            {
                value = _UserContext.UserId;
            }
            else if (condition.Value == "Blank")
            {
                value = null;
            }
            else
            {
                value = int.Parse(condition.Value);
            }
            return value;
        }
    }
}
