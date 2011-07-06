using System;
using System.IO;
using System.Web;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Configuration
{
    public class ProfilerConfiguration :  IDoNotWantToBeProfiled
    {
        public long GetRequestThreshold { get; set; }
        public long PostRequestThreshold { get; set; }
        public bool MonitoringEnabled { get; set; }
        public bool CaptureExceptions { get; set; }
        public bool CaptureResponse { get; set; }
        public Func<HttpContext, Stream> ResponseFilter { get; set; }
        public MethodDataCollectorMappings MethodDataCollectorMappings { get; set; }
        public IContainer Container { get; set; }
        public Func<HttpRequest, bool> ShouldProfile { get; set; }
        public Func<HttpContext, bool> AuthorizedForManagement { get; set; }
        public Action<Exception> ReportException { get; set; }

        public ProfilerConfiguration()
        {
            MethodDataCollectorMappings = new MethodDataCollectorMappings();
        }
    }
}