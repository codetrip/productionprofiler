using System.Web;

namespace ProductionProfiler.Interfaces
{
    public interface IRequestProfilingCoordinator
    {
        void BeginRequest(HttpContext context);
        void EndRequest(HttpContext context);
    }
}