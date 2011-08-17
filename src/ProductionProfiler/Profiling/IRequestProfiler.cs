using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public interface IRequestProfiler : IDoNotWantToBeProfiled
    {
        void MethodEntry(MethodInvocation invocation);
        void MethodExit(MethodInvocation invocation);
        void Start(HttpContext context, IEnumerable<IProfilingCoordinator> coordinators);
        void Stop(HttpResponse response);
    }
}
