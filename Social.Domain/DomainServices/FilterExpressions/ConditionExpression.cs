using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Framework.Core;

namespace Social.Domain.DomainServices
{
    public abstract class ConditionExpression : IConditionExpression
    {
        private ExpressionBuildOptions _options { get; set; }

        public IConditionExpression SetOptions(ExpressionBuildOptions options)
        {
            _options = options;
            return this;
        }

        public IDependencyResolver DependencyResolver { get; set; }

        public abstract Expression<Func<Conversation, bool>> Build(FilterCondition condition);

        public abstract bool IsMatch(FilterCondition condition);

        protected int GetUserId()
        {
            if (_options != null && _options.UserId > 0)
            {
                return _options.UserId;
            }
            else
            {
                Checker.NotNull(DependencyResolver, nameof(DependencyResolver));
                var userContext = DependencyResolver.Resolve<IUserContext>();
                return userContext.UserId;
            }
        }

        protected int[] GetMyDepartmentMembers()
        {
            if (_options != null && _options.MyDepartmentMembers != null)
            {
                return _options.MyDepartmentMembers;
            }
            else
            {
                Checker.NotNull(DependencyResolver, nameof(DependencyResolver));
                var departmentService = DependencyResolver.Resolve<IDepartmentService>();
                return departmentService.GetMyDepartmentMembers(GetUserId());
            }
        }

        protected int GetMyDepartmentId()
        {
            if (_options != null && _options.MyDepartmentId > 0)
            {
                return _options.MyDepartmentId;
            }
            else
            {
                Checker.NotNull(DependencyResolver, nameof(DependencyResolver));
                var departmentService = DependencyResolver.Resolve<IDepartmentService>();
                return departmentService.GetMyDepartmentId(GetUserId());
            }
        }
    }
}
