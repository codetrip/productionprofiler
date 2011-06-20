
using System.Web;
using ProductionProfiler.Core.Interfaces;
using ProductionProfiler.Core.Interfaces.Entities;

namespace ProductionProfiler.Core.Handlers
{
    public class ErrorRequestHandler : IRequestHandler
    {
        public void HandleRequest(HttpContext context, RequestInfo requestInfo)
        {
            context.Response.Write("<html><body><h1>Invalid request, no suitable handler found to process your request</h1></body></html>");
        }
    }
}
