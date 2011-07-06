
using System;

namespace ProductionProfiler.Core.Profiling.Entities
{
    public class ProfiledRequestDataWrapper
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime CapturedOnUtc { get; set; }
        public byte[] Data { get; set; }
    }
}
