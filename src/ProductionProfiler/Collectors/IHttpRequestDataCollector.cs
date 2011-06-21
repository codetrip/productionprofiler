using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public interface IHttpRequestDataCollector
    {
        List<DataCollection> Collect(HttpRequest request);
    }
}