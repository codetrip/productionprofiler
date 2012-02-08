using System;

namespace ProductionProfiler.Core.Profiling.Entities
{
    [Serializable]
    public class ProfilerMessage
    {
        public long Milliseconds { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
    }
}