using System;
using System.Collections.Generic;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Logging
{
    public class Log4NetLogger : ILogger
    {
        private static readonly IList<Log4NetProfilingAppender> _profilingAppenders = new List<Log4NetProfilingAppender>();
        private MethodData _currentMethod;

        public void Start()
        {
            foreach (var appender in _profilingAppenders)
                appender.AppendLoggingEvent += ProfilingAppenderAppendLoggingEvent;
        }

        public void Stop()
        {
            foreach (var appender in _profilingAppenders)
                appender.AppendLoggingEvent -= ProfilingAppenderAppendLoggingEvent;
        }

        public void Initialise()
        {
            foreach (var log in LogManager.GetCurrentLoggers())
            {
                var logger = log.Logger as Logger;

                if (logger != null)
                {
                    var profilingAppender = new Log4NetProfilingAppender
                    {
                        Threshold = Level.Debug,
                        Name = "{0}-{1}".FormatWith(logger.Name, Constants.ProfilingAppender)
                    };

                    logger.AddAppender(profilingAppender);
                    _profilingAppenders.Add(profilingAppender);
                }
            }

            Hierarchy repository = LogManager.GetRepository() as Hierarchy;

            if (repository != null)
            {
                repository.Configured = true;
                repository.RaiseConfigurationChanged(EventArgs.Empty);
            }
        }

        public MethodData CurrentMethod
        {
            set { _currentMethod = value; }
        }

        private void ProfilingAppenderAppendLoggingEvent(object sender, AppendLoggingEventEventArgs e)
        {
            if (_currentMethod != null)
            {
                _currentMethod.Messages.Add(e.LoggingEvent.ToLogMessage(_currentMethod.Elapsed()));

                if (e.LoggingEvent.ExceptionObject != null)
                {
                    _currentMethod.Exceptions.AddException(e.LoggingEvent.ExceptionObject, _currentMethod.Elapsed());
                }
                else if (e.LoggingEvent.MessageObject is Exception)
                {
                    _currentMethod.Exceptions.AddException(e.LoggingEvent.MessageObject as Exception, _currentMethod.Elapsed());
                }
            }
        }
    }
}
