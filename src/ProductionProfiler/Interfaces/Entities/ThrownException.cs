using System;

namespace ProductionProfiler.Core.Interfaces.Entities
{
    [Serializable]
    public class ThrownException
    {
        public long Milliseconds { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string CallStack { get; set; }
    }
}