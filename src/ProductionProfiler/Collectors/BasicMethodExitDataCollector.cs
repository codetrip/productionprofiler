
using System;
using System.Collections.Generic;
using System.Linq;
using ProductionProfiler.Core.Dynamic;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public class BasicMethodExitDataCollector : IMethodExitDataCollector
    {
        public List<DataCollection> Collect(MethodInvocation invocation)
        {
            if (CollectionTypes != null && CollectionTypes.Any(t => t == invocation.TargetType))
            {
                if (invocation.ReturnValue != null)
                {
                    var methodInfo = new DataCollection
                    {
                        Name = "Method Exit",
                        Data = new List<DataCollectionItem>
                        {
                            new DataCollectionItem("Return Type", invocation.ReturnValue.GetType().FullName),
                            new DataCollectionItem("Return Value", invocation.ReturnValue.GetSummary().ToString())
                        }
                    };

                    return new List<DataCollection>(new[] { methodInfo });
                }
            }

            return null;
        }

        public IEnumerable<Type> CollectionTypes { get; set; }
    }
}
