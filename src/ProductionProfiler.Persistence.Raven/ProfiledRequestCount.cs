using System;

namespace ProductionProfiler.Persistence.Raven
{
    public class ProfiledRequestCount
    {
        public string Url { get; set; }
        public int Count { get; set; }
        public DateTime MostRecentUtc { get; set; }
    }
}