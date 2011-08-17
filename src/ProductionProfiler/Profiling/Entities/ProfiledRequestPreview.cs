using System;

namespace ProductionProfiler.Core.Profiling.Entities
{
    public class ProfiledRequestDataPreview
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public DateTime CapturedOnUtc { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string Server { get; set; }
    }

    [Serializable]
    public class EncodedProfiledRequestDataPreview : ProfiledRequestDataPreview
    {
        public string EncodedUrl { get; set; }
    }
}
