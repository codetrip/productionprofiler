using System.Collections.Generic;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public interface IMethodEntryDataCollector
    {
        List<DataCollection> Collect(MethodInvocation invocation);
    }
}
