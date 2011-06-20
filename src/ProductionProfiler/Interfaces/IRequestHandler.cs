
using System.Web;
using ProductionProfiler.Core.Interfaces.Entities;

namespace ProductionProfiler.Core.Interfaces
{
    public interface IRequestHandler
    {
        void HandleRequest(HttpContext context, RequestInfo requestInfo);
    }
}
