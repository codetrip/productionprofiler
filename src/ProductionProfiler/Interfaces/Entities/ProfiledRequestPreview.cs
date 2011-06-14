using System;

namespace ProductionProfiler.Interfaces.Entities
{
    public class ProfiledRequestDataPreview
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime CapturedOnUtc { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string Server { get; set; }
    }
}
