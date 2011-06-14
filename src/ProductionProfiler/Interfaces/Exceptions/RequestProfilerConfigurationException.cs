using System;
using System.Runtime.Serialization;

namespace ProductionProfiler.Interfaces.Exceptions
{
    public class RequestProfilerConfigurationException : Exception
    {
        public RequestProfilerConfigurationException()
        {}

        public RequestProfilerConfigurationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public RequestProfilerConfigurationException(string message)
            : base (message)
        {}

        public RequestProfilerConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}
