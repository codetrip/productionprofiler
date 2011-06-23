using System.Web;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Handlers
{
    public interface IRequestHandler : IDoNotWantToBeProfiled
    {
        void HandleRequest(HttpContext context, RequestInfo requestInfo);
    }
}
