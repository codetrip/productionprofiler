using System.Web;

namespace ProductionProfiler.Profiling
{
    public interface IRequestProfilingManager
    {
        void BeginRequest(HttpContext context);
        void EndRequest(HttpContext context);
    }
}