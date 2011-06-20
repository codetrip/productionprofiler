using System;

namespace ProductionProfiler.Core.Interfaces.Entities
{
    [Serializable]
    public class ProfilerMessage
    {
        public long Milliseconds { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
    }
}