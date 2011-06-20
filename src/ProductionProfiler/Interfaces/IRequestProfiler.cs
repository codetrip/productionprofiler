using System;
using System.Web;
using ProductionProfiler.Core.Interfaces.Entities;

namespace ProductionProfiler.Core.Interfaces
{
    public interface IRequestProfiler
    {
        Guid RequestId { get; set; }
        bool InitialisedForRequest { get; set; }
        void MethodEntry(string methodName);
        void MethodExit();
        void StartProfiling(HttpRequest request);
        ProfiledRequestData StopProfiling();
        void ProfilerError(string message);
    }
}
