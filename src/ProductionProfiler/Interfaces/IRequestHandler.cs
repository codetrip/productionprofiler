
using System.Web;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Interfaces
{
    public interface IRequestHandler
    {
        void HandleRequest(HttpContext context, RequestInfo requestInfo);
    }
}
