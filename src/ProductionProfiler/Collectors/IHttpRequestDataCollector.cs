using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public interface IHttpRequestDataCollector : IDoNotWantToBeProxied
    {
        List<DataCollection> Collect(HttpRequest request);
    }
}