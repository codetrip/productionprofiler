using System;
using System.Text;
using System.Text.RegularExpressions;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Auditing
{
    public abstract class ComponentAuditorBase : IComponentAuditor
    {
        protected Guid RequestId { get; set; }

        private void CommonAuditEventPopulation(Type component, AuditEvent auditEvent)
        {
            auditEvent.Component = component.Name;
            auditEvent.Description = Description;
            auditEvent.Timestamp = DateTime.UtcNow;
        }

        private AuditEvent CreateEventForException(Type component, Exception exception, int eventId = 0)
        {
            var auditEvent = new ExceptionAuditEvent
            {
                Exception = exception,
                EventId = eventId,
            };

            CommonAuditEventPopulation(component, auditEvent);

            auditEvent.Message = ExceptionUtility.FormatException(exception, appendEnvironmentInfo: true, reflectException: true, isAuditOutput: true);
            auditEvent.FormattedMessage = auditEvent.Message;

            return auditEvent;
        }


        /// <summary>
        /// This should create a new instance of an <see cref="AuditEvent"/> 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventId"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private AuditEvent CreateEvent(Type component, int eventId, string message, params object[] args)
        {
            string formattedMessage;

            try
            {
                formattedMessage = string.Format(message, args);
            }
            catch (Exception e)
            {
                //make sure there are no format markers in the message
                formattedMessage = Regex.Replace(message, "{([0-9]*)}", string.Empty);

                // if we get an exception when creating the AuditEvent it is because string.format()
                // blew up.  Hence we want to audit the message with no parameters (so that something gets audited)
                // and also produce an error so we can fix this in the future.
                var argText = new StringBuilder();
                foreach (var arg in args)
                    argText.Append(arg).Append('|');

                Error(component, new Exception("Exception when creating AuditEvent. Message: " + formattedMessage + " Args: " + argText, e));
            }

            var auditEvent = new AuditEvent
            {
                Message = formattedMessage,
                EventId = eventId,
                FormattedMessage = Description.IsNullOrEmpty() ? 
                    string.Format("[{0}] {1}", component.Name, formattedMessage) : 
                    string.Format("[{0}::{1}] {2}", Description, component.Name, formattedMessage)
            };

            CommonAuditEventPopulation(component, auditEvent);

            return auditEvent;
        }

        protected abstract void DoTrace(AuditEvent auditEvent);
        protected abstract void DoInfo(AuditEvent auditEvent);
        protected abstract void DoWarning(AuditEvent auditEvent);
        protected abstract void DoError(AuditEvent auditEvent);
        public abstract bool IsDebugEnabled { get; }

        #region Implementation of IComponentAuditor

        public string Description { get; set; }

        public void Trace(Type component, string message, params object[] args)
        {
            DoTrace(CreateEvent(component, 0, message, args));
        }

        public void Info(Type component, string message, params object[] args)
        {
            DoInfo(CreateEvent(component, 0, message, args));
        }

        public void Warning(Type component, string message, params object[] args)
        {
            DoWarning(CreateEvent(component, 0, message, args));
        }

        public void Error(Type component, string message, params object[] args)
        {
            Error(component, 0, message, args);
        }

        public void Error(Type component, int eventId, string message, params object[] args)
        {
            DoError(CreateEvent(component, eventId, message, args));
        }

        public void Error(Type component, Exception e)
        {
            Error(component, 0, e);
        }

        public void Error(Type component, int eventId, Exception e)
        {
            DoError(CreateEventForException(component, e, eventId));
        }

        #endregion
    }
}
