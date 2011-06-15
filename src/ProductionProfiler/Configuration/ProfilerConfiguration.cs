using ProductionProfiler.Log4Net;

namespace ProductionProfiler.Configuration
{
    public class ProfilerConfiguration
    {
        public long GetRequestThreshold { get; set; }
        public long PostRequestThreshold { get; set; }
        public bool Log4NetEnabled { get; set; }
        public Log4NetProfilingAppender ProfilingAppender { get; set; }
    }
}