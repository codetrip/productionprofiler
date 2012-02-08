using log4net.Core;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Logging
{
    public static class LoggingEventExtensions
    {
        public static ProfilerMessage ToLogMessage(this LoggingEvent loggingEvent, long elapsedMillseconds)
        {
            return new ProfilerMessage
            {
                Logger = loggingEvent.LoggerName,
                Message = loggingEvent.RenderedMessage,
                Milliseconds = elapsedMillseconds,
                Level = loggingEvent.Level.ToString()
            };
        }
    }
}
