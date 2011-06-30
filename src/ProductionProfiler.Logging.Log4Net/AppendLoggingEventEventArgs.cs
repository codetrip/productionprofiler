using System;
using log4net.Core;

namespace ProductionProfiler.Logging.Log4Net
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
