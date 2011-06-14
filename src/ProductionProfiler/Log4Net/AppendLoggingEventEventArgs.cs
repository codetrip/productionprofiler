using System;
using log4net.Core;

namespace ProductionProfiler.Log4Net
{
    public class AppendLoggingEventEventArgs : EventArgs
    {
        public LoggingEvent LoggingEvent;

        public AppendLoggingEventEventArgs(LoggingEvent loggingEvent)
        {
            LoggingEvent = loggingEvent;
        }
    }
}
