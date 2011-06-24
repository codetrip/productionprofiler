using System;

namespace ProductionProfiler.Core.Profiling.Entities
{
    public class ProfiledRequestPreview
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime CapturedOnUtc { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string Server { get; set; }
    }
}
