
using System;
using log4net.Appender;
using log4net.Core;

namespace ProductionProfiler.Log4Net
{
    public class Log4NetProfilingAppender : AppenderSkeleton
    {
        private readonly object _eventlock = new object();
        private event EventHandler<AppendLoggingEventEventArgs> _appendLoggingEvent;

        private void OnAppendLoggingEvent(AppendLoggingEventEventArgs e)
        {
            EventHandler<AppendLoggingEventEventArgs> handler = _appendLoggingEvent;
            if (handler != null) 
                handler(this, e);
        }
        
        public event EventHandler<AppendLoggingEventEventArgs> AppendLoggingEvent
        {
            //Always explicitly define the add/remove methods as the CLR obtains a lock 
            //on the Log4NetProfilingAppender object instance if you do not which can cause thread sychronisation 
            //problems (it is always recommended you use a private object to obtain a lock)
            add
            {
                lock (_eventlock)
                {
                    _appendLoggingEvent += value;
                }
            }
            remove
            {
                lock (_eventlock)
                {
                    _appendLoggingEvent -= value;
                }
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            OnAppendLoggingEvent(new AppendLoggingEventEventArgs(loggingEvent));
        }
    }
}
