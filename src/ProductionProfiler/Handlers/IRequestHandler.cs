using System.Web;
using ProductionProfiler.Core.Handlers.Entities;

namespace ProductionProfiler.Core.Handlers
{
    public interface IRequestHandler
    {
        void HandleRequest(HttpContext context, RequestInfo requestInfo);
    }
}
