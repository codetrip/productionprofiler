using System;
using log4net.Core;

namespace ProductionProfiler.Core.Logging
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
