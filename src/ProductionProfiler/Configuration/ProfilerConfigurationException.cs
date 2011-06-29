using System;
using System.Runtime.Serialization;

namespace ProductionProfiler.Core.Configuration
{
    [Serializable]
    public class ProfilerConfigurationException : Exception
    {
        public ProfilerConfigurationException()
        { }

        public ProfilerConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public ProfilerConfigurationException(string message)
            : base(message)
        { }
    }
}
