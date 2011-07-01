using System;
using System.IO;
using System.Web;
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

        public ProfilerConfiguration()
        {
            MethodDataCollectorMappings = new MethodDataCollectorMappings();
        }
    }
}