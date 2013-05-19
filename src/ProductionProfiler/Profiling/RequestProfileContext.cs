using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    /// <summary>
    /// Contains info about a particular profiled request.
    /// </summary>
    public class RequestProfileContext
    {
        private readonly Stopwatch _stopwatch;

        public RequestProfileContext(HttpContext httpContext, IEnumerable<IProfilingTrigger> triggersForCurrentRequest)
        {
            HttpContext = httpContext;
            Triggers = triggersForCurrentRequest;
            _stopwatch = new Stopwatch();
        }

        public void StartTiming()
        {
            _stopwatch.Start();
        }

        public HttpContext HttpContext { get; private set; }
        public IEnumerable<IProfilingTrigger> Triggers { get; private set; }
        public TimeSpan RequestDuration { get { return _stopwatch.Elapsed; } }
        public ProfiledRequestData ProfiledRequestData { get; set; }
    }
}