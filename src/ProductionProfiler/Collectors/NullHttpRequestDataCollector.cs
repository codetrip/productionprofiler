using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public class NullHttpRequestDataCollector : IHttpRequestDataCollector
    {
        public List<DataCollection> Collect(HttpRequest request)
        {
            return null;
        }
    }
}
