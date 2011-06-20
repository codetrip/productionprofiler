using System.Web;

namespace ProductionProfiler.Core.Profiling
{
    public interface IRequestProfilingCoordinator
    {
        void BeginRequest(HttpContext context);
        void EndRequest(HttpContext context);
    }
}