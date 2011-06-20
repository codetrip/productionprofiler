
using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public class BasicHttpResponseDataCollector : IHttpResponseDataCollector
    {
        public List<DataCollection> Collect(HttpResponse response)
        {
            var data = new List<DataCollection>();

            if (HttpRuntime.UsingIntegratedPipeline && response.Headers.Count > 0)
            {
                data.Add(new DataCollection("Response Headers", response.Headers));
            }

            var responseData = new DataCollection("Response Data");

            //responseData.Data.Add("Body", new StreamReader(response.OutputStream).ReadToEnd());
            responseData.Data.Add(new DataCollectionItem("StatusCode", response.StatusCode.ToString()));

            data.Add(responseData);
            return data;
        }
    }
}
