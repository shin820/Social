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
    public class DepartmentAssigneeStatusExpression : OptionExpression
    {
        private IDepartmentService _DepartmentService;
        public DepartmentAssigneeStatusExpression(IDepartmentService DepartmentService) : base("Department Assignee Status", "")
        {
            _DepartmentService = DepartmentService;
        }

        protected override object GetValue(string value)
        {
            return null;
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            int[] DepartmentIds = _DepartmentService.IsMatchStatusDepartments(int.Parse(condition.Value));
            var predicate = PredicateBuilder.New<Conversation>();
            foreach (int member in DepartmentIds)
            {
                Expression<Func<Conversation, bool>> b = t => (int)t.DepartmentId == member;
                predicate = predicate.Or(b);
            }
            return predicate;
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            int[] DepartmentIds = _DepartmentService.IsMatchStatusDepartments(int.Parse(condition.Value));
            var predicate = PredicateBuilder.New<Conversation>();
            foreach (int member in DepartmentIds)
            {
                Expression<Func<Conversation, bool>> b = t => (int)t.DepartmentId != member;
                predicate = predicate.And(b);
            }
            return predicate;
        }
    }
}
