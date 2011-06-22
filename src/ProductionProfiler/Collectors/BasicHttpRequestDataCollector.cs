
using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Collectors
{
    public class BasicHttpRequestDataCollector : IHttpRequestDataCollector
    {
        public List<DataCollection> Collect(HttpRequest request)
        {
            var data = new List<DataCollection>();

            if(request.Form.Count > 0)
            {
                data.Add(new DataCollection("Form Params", request.Form));
            } 
            
            if (request.Headers.Count > 0)
            {
                data.Add(new DataCollection("Request Headers", request.Headers));
            }

            if(request.Cookies.Count > 0)
            {
                var cookies = new DataCollection("Request Cookies");
                foreach (string key in request.Cookies)
                {
                    var requestCookie = request.Cookies[key];
                    if(requestCookie != null)
                        cookies.Data.Add(new DataCollectionItem(requestCookie.Name, requestCookie.Value));
                }
                data.Add(cookies);
            }

            return data;
        }
    }
}
