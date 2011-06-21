using System.Collections.Generic;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public class NullMethodExitDataCollector : IMethodExitDataCollector
    {
        public List<DataCollection> Collect(MethodInvocation invocation)
        {
            return null;
        }
    }
}
