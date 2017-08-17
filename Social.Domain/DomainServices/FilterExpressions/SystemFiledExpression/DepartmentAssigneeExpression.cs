using Framework.Core;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public class DepartmentAssigneeExpression : OptionExpression
    {
        private IDepartmentService _departmentService;
        private IUserContext _userContext;
        public DepartmentAssigneeExpression(IUserContext userContext,  IDepartmentService departmentService) : base("Department Assignee", "")
        {
            _departmentService = departmentService;
            _userContext = userContext;
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            int? value = MatchValue(condition);
            return t => t.DepartmentId.HasValue && t.DepartmentId.Value == value;
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            int? value = MatchValue(condition);
            return t => t.DepartmentId.HasValue && t.DepartmentId.Value != value || !t.DepartmentId.HasValue;
        }

        protected override object GetValue(string rawValue)
        {
            return null;
        }

        private int? MatchValue(FilterCondition condition)
        {
            int value = default(int);
            if (condition.Value == "@My Department")
            {
                value = _departmentService.GetMyDepartmentId(_userContext.UserId);
            }
            else if (condition.Value == "Blank")
            {
                return null;
            }
            else
            {
                value = int.Parse(condition.Value);
            }
            return value;
        }
    }
}
