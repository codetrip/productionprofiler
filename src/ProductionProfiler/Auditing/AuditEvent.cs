using System;

namespace ProductionProfiler.Core.Auditing
{
    public class ExceptionAuditEvent : AuditEvent
    {
        public Exception Exception { get; set; }
    }

    public class AuditEvent
    {
        /// <summary>
        /// The application that generated the event.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The component that this event is associated with
        /// </summary>
		public string Component { get; set; }

		/// <summary>
		/// The parent component that this event is associated with
		/// </summary>
		public string Description { get; set; }

        /// <summary>
        /// The datetime that this event occurred (usually utc)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The message to record
        /// </summary>
        public string Message { get; set; }

		/// <summary>
		/// The formatted message to record
		/// </summary>
		public string FormattedMessage { get; set; }

        /// <summary>
        /// An EventId property to associate with the audit
        /// </summary>
        public int EventId { get; set; }
    }
}