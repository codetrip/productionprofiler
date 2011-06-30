using log4net.Core;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Logging.Log4Net
{
    public static class LoggingEventExtensions
    {
        public static ProfilerMessage ToLogMessage(this LoggingEvent loggingEvent, long elapsedMillseconds)
        {
            return new ProfilerMessage
            {
                Message = loggingEvent.RenderedMessage,
                Milliseconds = elapsedMillseconds,
                Level = loggingEvent.Level.ToString()
            };
        }
    }
}
