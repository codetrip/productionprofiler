using System;

namespace ProductionProfiler.Core.Configuration
{
    public class SamplingConfiguration
    {
        public TimeSpan SampleFrequency { get; set; }
        public TimeSpan SamplePeriod { get; set; }
    }
}
