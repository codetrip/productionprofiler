using System.Web;

namespace ProductionProfiler.Core.Interfaces
{
    public interface IRequestProfilingCoordinator
    {
        void BeginRequest(HttpContext context);
        void EndRequest(HttpContext context);
    }
}