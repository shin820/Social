using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;

namespace Framework.Core
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition
                ? query.Where(predicate)
                : query;
        }

        public static PagedList<TDto> PagingAndMapping<TEntity, TDto>(this IQueryable<TEntity> query, Pager pager) where TEntity : Entity
        {
            int totalCount;
            var items = query.Paging(pager, out totalCount).ProjectTo<TDto>().ToList();

            return new PagedList<TDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public static IQueryable<TEntity> Paging<TEntity>(this IQueryable<TEntity> query, Pager pager, out int totalCount) where TEntity : Entity
        {
            if (pager.PageSize <= 0)
            {
                pager.PageSize = 20;
            }

            totalCount = query.Count();
            if (totalCount <= pager.PageSize || pager.Page <= 0)
            {
                pager.Page = 1;
            }

            int skipRows = (pager.Page - 1) * pager.PageSize;

            if (string.IsNullOrWhiteSpace(pager.OrderBy))
            {
                query = pager.IsAsc ? query.OrderBy(t => t.Id) : query.OrderByDescending(t => t.Id);
            }
            else
            {
                query = pager.IsAsc ? query.OrderBy(pager.OrderBy) : query.OrderBy(pager.OrderBy + " descending");
            }

            return query.Skip(skipRows).Take(pager.PageSize);
        }
    }
}
