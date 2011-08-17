
using System;

namespace ProductionProfiler.Core.Profiling.Entities
{
    public class ProfiledRequestDataWrapper
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string SessionUserId { get; set; }
        public Guid SamplingId { get; set; }
        public string Url { get; set; }
        public DateTime CapturedOnUtc { get; set; }
        public byte[] Data { get; set; }
    }
}
