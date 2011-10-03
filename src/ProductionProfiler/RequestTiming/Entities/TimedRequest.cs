using System;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.RequestTiming.Entities
{
    [Serializable]
    public class TimedRequest : IAsyncPersistable
    {
        public TimedRequest(string url, long durationMs) :this()
        {
            Url = url;
            DurationMs = durationMs;
            RequestUtc = DateTime.UtcNow;
            Server = Environment.MachineName;
            UrlPathAndQuery = new Uri(url).PathAndQuery;
        }

        public TimedRequest()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string UrlPathAndQuery { get; set; }
        public long DurationMs { get; set; }
        public string Server { get; set; }

        public string FriendlyRequestLocal
        {
            get { return RequestUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public DateTime RequestUtc { get; set; }
    }
}