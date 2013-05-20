using System;
using log4net;

namespace ProductionProfiler.Core.Auditing
{
    public class Log4NetComponentAuditor : ComponentAuditorBase
    {
        private const string DefaultLoggerName = "profiler";
        private readonly ILog _log;

        public Log4NetComponentAuditor()
            : this(DefaultLoggerName)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetComponentAuditor"/> class.
        /// </summary>
        public Log4NetComponentAuditor(string loggerName) 
        {
            _log = LogManager.GetLogger(loggerName);
            RequestId = Guid.NewGuid();
        }

        protected override void DoTrace(AuditEvent auditEvent)
        {
            _log.Debug(auditEvent.FormattedMessage);
        }

        protected override void DoInfo(AuditEvent auditEvent)
        {
            _log.Info(auditEvent.FormattedMessage);
        }

        protected override void DoWarning(AuditEvent auditEvent)
        {
            _log.Warn(auditEvent.FormattedMessage);
        }

        protected override void DoError(AuditEvent auditEvent)
        {
            _log.Error(auditEvent.FormattedMessage);
        }

        public override bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }
    }
}