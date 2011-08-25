using System;
using System.Collections.Generic;
using System.Linq;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Extensions
{
    public static class ThrownExceptionExtensions
    {
        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <param name="exceptions">The exceptions.</param>
        /// <param name="e"></param>
        /// <param name="elapsedMilliseconds"></param>
        /// <returns></returns>
        public static void AddException(this List<ThrownException> exceptions, Exception e, long elapsedMilliseconds)
        {
            //try not to add the same expception twice
            if (!exceptions.Any(ex => ex.Type == e.GetType().FullName))
            {
                exceptions.Add(new ThrownException
                {
                    Message = e.Format(),
                    Milliseconds = elapsedMilliseconds,
                    Type = e.GetType().ToString()
                });
            }
        }
    }
}
