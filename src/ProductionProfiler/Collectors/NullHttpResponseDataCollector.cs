using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public class NullHttpResponseDataCollector : IHttpResponseDataCollector
    {
        public List<DataCollection> Collect(HttpResponse request)
        {
            return null;
        }
    }
}
