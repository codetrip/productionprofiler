
using System.Collections.Generic;
using ProductionProfiler.Core.Dynamic;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public class BasicMethodEntryDataCollector : IMethodEntryDataCollector
    {
        public List<DataCollection> Collect(MethodInvocation invocation)
        {
            var data = new List<DataCollection>();
            var methodInfo = new DataCollection("Method Entry");

            int count = 1;
            foreach (object argument in invocation.Arguments)
            {
                methodInfo.Data.Add(new DataCollectionItem("Argument " + count, argument.GetSummary().ToString()));
                count++;
            }

            return data;
        }
    }
}
