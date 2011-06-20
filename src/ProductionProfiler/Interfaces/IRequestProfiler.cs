using System;
using System.Web;
using Castle.DynamicProxy;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Interfaces
{
    public interface IRequestProfiler
    {
        Guid RequestId { get; set; }
        bool InitialisedForRequest { get; set; }
        void MethodEntry(IInvocation invocation);
        void MethodExit();
        void StartProfiling(HttpRequest request);
        ProfiledRequestData StopProfiling();
        void ProfilerError(string message);
    }
}
