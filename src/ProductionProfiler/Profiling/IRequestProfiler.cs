using System;
using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public interface IRequestProfiler
    {
        Guid RequestId { get; set; }
        bool InitialisedForRequest { get; set; }
        void MethodEntry(MethodInvocation invocation);
        void MethodExit(MethodInvocation invocation);
        void StartProfiling(HttpContext context);
        ProfiledRequestData StopProfiling(HttpResponse response);
        void ProfilerError(string message);
    }
}
