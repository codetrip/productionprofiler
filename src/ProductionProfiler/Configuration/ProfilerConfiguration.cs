using System;
using System.IO;
using System.Web;

namespace ProductionProfiler.Core.Configuration
{
    public class ProfilerConfiguration
    {
        public long GetRequestThreshold { get; set; }
        public long PostRequestThreshold { get; set; }
        public bool MonitoringEnabled { get; set; }
        public bool CaptureExceptions { get; set; }
        public bool CaptureResponse { get; set; }
        public Func<HttpContext, Stream> GetResponseFilter { get; set; }
        public MethodDataCollectorMappingConfiguration MethodDataCollectorMappings { get; set; }

        public ProfilerConfiguration()
        {
            MethodDataCollectorMappings = new MethodDataCollectorMappingConfiguration();
        }
    }
}