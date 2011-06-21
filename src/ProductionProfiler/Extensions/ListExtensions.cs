
using System.Collections.Generic;

namespace ProductionProfiler.Core.Extensions
{
    public static class ListExtensions
    {
        public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T> items)
        {
            if(items != null)
                list.AddRange(items);
        }
    }
}
