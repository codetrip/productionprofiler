
using log4net.Core;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Extensions
{
    public static class LoggingEventExtensions
    {
        public static LogMessage ToLogMessage(this LoggingEvent loggingEvent, long elapsedMillseconds)
        {
            return new LogMessage
            {
                Domain = loggingEvent.Domain,
                Message = loggingEvent.RenderedMessage,
                Milliseconds = elapsedMillseconds,
                Level = loggingEvent.Level.ToString()
            };
        }
    }
}
