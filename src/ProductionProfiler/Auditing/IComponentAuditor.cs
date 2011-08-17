
using System;

namespace ProductionProfiler.Core.Auditing
{
    public interface IComponentAuditor
    {
        /// <summary>
        /// Description of the Auditor, this is usually in a web context the name of the controller which initiated the process.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// This should publish an individual audit event if trace switch is configured to Verbose
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        void Trace(Type component, string message, params object[] args);

        /// <summary>
        /// This should publish an individual audit event if trace switch is configured to Info
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        void Info(Type component, string message, params object[] args);

        /// <summary>
        /// This should publish an individual audit event if trace switch is configured to Warning
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        void Warning(Type component, string message, params object[] args);

        /// <summary>
        /// This should publish an individual audit event if trace switch is configured to Error
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        void Error(Type component, string message, params object[] args);

        /// <summary>
        /// This should publish an individual audit event if trace switch is configured to Error
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="eventId"></param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        void Error(Type component, int eventId, string message, params object[] args);

        /// <summary>
        /// This should publish an individual audit event if trace switch is configured to Error
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="e">The e.</param>
        void Error(Type component, Exception e);

        /// <summary>
        /// This should publish an individual audit event if trace switch is configured to Error
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="eventId"></param>
        /// <param name="e">The e.</param>
        void Error(Type component, int eventId,  Exception e);

        /// <summary>
        /// Indicates whether debug is enabled for the underlying logging framework
        /// Allows us to perform expensive debug related operations conditionally 
        /// </summary>
        bool IsDebugEnabled { get; }
    }
}