using System;
using log4net.Core;

namespace ProductionProfiler.Core.Log4Net
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
