using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductionProfiler.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Combines the contents of the two byte arrays and returns the resulting byte[]
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortAscending"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> SetSort<T, TSort>(this IEnumerable<T> enumerable, Func<T, TSort> sortExpression, bool sortAscending)
        {
            if (sortAscending)
                return enumerable.OrderBy(sortExpression);

            return enumerable.OrderByDescending(sortExpression);
        }
    }
}
