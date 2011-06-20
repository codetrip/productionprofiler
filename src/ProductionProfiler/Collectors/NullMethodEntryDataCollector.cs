using System.Collections.Generic;
using System.Collections.Specialized;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public class NullMethodEntryDataCollector : IMethodEntryDataCollector
    {
        public List<DataCollection> Collect(MethodInvocation invocation)
        {
            return null;
        }
    }
}
