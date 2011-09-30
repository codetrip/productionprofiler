using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ProductionProfiler.Core.Extensions
{
    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> SetSort<T, TSort>(this IQueryable<T> queryable, Expression<Func<T, TSort>> sortExpression, bool sortAscending)
        {
            if (sortAscending)
                return queryable.OrderBy(sortExpression);

            return queryable.OrderByDescending(sortExpression);
        }
    }
}
