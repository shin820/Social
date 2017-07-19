using Framework.Core;
using LinqKit;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.FilterExpressions
{
    public class FilterExpressionFactory : IFilterExpressionFactory
    {
        private IList<IConditionExpression> _conditionExpressions;

        public FilterExpressionFactory(IDependencyResolver dependencyResover)
        {
            _conditionExpressions = dependencyResover.ResolveAll<IConditionExpression>();
        }


        public Expression<Func<Conversation, bool>> Create(Filter filter)
        {
            var expressions = filter.Conditions.Select(t => GetConditionExpression(t)).Where(t => t != null).ToList();

            if (filter.Type == FilterType.All)
            {
                var predicate = PredicateBuilder.New<Conversation>();
                foreach (var expression in expressions)
                {
                    predicate = predicate.And(expression);
                }

                return predicate;
            }

            if (filter.Type == FilterType.Any)
            {
                var predicate = PredicateBuilder.New<Conversation>();
                foreach (var expression in expressions)
                {
                    predicate = predicate.Or(expression);
                }

                return predicate;
            }

            return t => true;
        }

        private Expression<Func<Conversation, bool>> GetConditionExpression(FilterCondition condition)
        {
            foreach (var expression in _conditionExpressions)
            {
                if (expression.IsMatch(condition))
                {
                    return expression.Build(condition);
                }
            }

            return null;
        }
    }
}
