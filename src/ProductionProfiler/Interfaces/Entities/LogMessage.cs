using System;

namespace ProductionProfiler.Interfaces.Entities
{
    [Serializable]
    public class LogMessage
    {
        public long Milliseconds { get; set; }
        public string Message { get; set; }
        public string Domain { get; set; }
        public string Level { get; set; }
    }
}