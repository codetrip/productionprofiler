using System.Collections.Generic;

namespace ProductionProfiler.Core.Extensions
{
    public static class StackExtensions
    {
        public static T PeekIfItems<T>(this Stack<T> stack)
        {
            if (stack.Count > 0)
                return stack.Peek();

            return default(T);
        }
    }
}
