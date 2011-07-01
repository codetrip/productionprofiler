using System;
using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public interface IRequestProfiler : IDoNotWantToBeProfiled
    {
        Guid RequestId { get; set; }
        void MethodEntry(MethodInvocation invocation);
        void MethodExit(MethodInvocation invocation);
        void StartProfiling(HttpContext context);
        void StopProfiling(HttpResponse response);
    }
}
