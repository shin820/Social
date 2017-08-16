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
        private IDepartmentService _departmentService;
        public DepartmentAssigneeStatusExpression(IDepartmentService departmentService) : base("Department Assignee Status", "")
        {
            _departmentService = departmentService;
        }

        protected override object GetValue(string value)
        {
            return null;
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            int[] departmentIds = _departmentService.IsMatchStatusDepartments(int.Parse(condition.Value));
            var predicate = PredicateBuilder.New<Conversation>();
            foreach (int member in departmentIds)
            {
                Expression<Func<Conversation, bool>> b = t =>t.DepartmentId.HasValue && (int)t.DepartmentId.Value == member;
                predicate = predicate.Or(b);
            }
            return predicate;
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            int[] departmentIds = _departmentService.IsMatchStatusDepartments(int.Parse(condition.Value));
            var predicate = PredicateBuilder.New<Conversation>();
            foreach (int member in departmentIds)
            {
                Expression<Func<Conversation, bool>> b = t => t.DepartmentId.HasValue && (int)t.DepartmentId.Value != member;
                predicate = predicate.And(b);
            }
            return predicate;
        }
    }
}
