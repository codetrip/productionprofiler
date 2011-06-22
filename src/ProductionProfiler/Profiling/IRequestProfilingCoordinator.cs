using System.Web;

namespace ProductionProfiler.Core.Profiling
{
    public interface IRequestProfilingCoordinator : IDoNotWantToBeProxied
    {
        void BeginRequest(HttpContext context);
        void EndRequest(HttpContext context);
    }
}