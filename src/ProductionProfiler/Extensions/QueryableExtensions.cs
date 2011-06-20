using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductionProfiler.Core.Extensions
{
    public static class QueryableExtensions
    {
        public static IOrderedEnumerable<T> SetSort<T, TSort>(this IEnumerable<T> enumerable, Func<T, TSort> sortExpression, bool sortAscending)
        {
            if (sortAscending)
                return enumerable.OrderBy(sortExpression);

            return enumerable.OrderByDescending(sortExpression);
        }
    }
}
