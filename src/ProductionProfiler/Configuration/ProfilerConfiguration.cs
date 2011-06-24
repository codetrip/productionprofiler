using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ProductionProfiler.Core.Log4Net;

namespace ProductionProfiler.Core.Configuration
{
    public class ProfilerConfiguration
    {
        public long GetRequestThreshold { get; set; }
        public long PostRequestThreshold { get; set; }
        public bool Log4NetEnabled { get; set; }
        public bool MonitoringEnabled { get; set; }
        public IList<Log4NetProfilingAppender> ProfilingAppenders { get; set; }
        public bool CaptureExceptions { get; set; }
        public bool CaptureResponse { get; set; }
        public Func<HttpContext, Stream> GetResponseFilter { get; set; }
        public MethodDataCollectorMappingConfiguration MethodDataCollectorMappings { get; set; }

        public ProfilerConfiguration()
        {
            MethodDataCollectorMappings = new MethodDataCollectorMappingConfiguration();
            ProfilingAppenders = new List<Log4NetProfilingAppender>();
        }
    }
}