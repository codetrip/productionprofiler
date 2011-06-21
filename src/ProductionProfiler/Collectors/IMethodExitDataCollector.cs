using System.Collections.Generic;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public interface IMethodExitDataCollector
    {
        /// <summary>
        /// Collect any data from the MethodInvocation for the profile data
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        List<DataCollection> Collect(MethodInvocation invocation);
    }
}