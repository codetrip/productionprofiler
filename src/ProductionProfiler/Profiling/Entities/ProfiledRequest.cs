using System;

namespace ProductionProfiler.Core.Profiling.Entities
{
    [Serializable]
    public class ProfiledRequest
    {
        public Guid Id { get; set; }
        /// <summary>
        /// The URL to be monitored
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// The length of time the URL took to execute originally
        /// </summary>
        public long ElapsedMilliseconds { get; set; }
        /// <summary>
        /// The number of times remaining to profile this request
        /// </summary>
        public int ProfilingCount { get; set; }
        /// <summary>
        /// The date/time this URL was profiled
        /// </summary>
        public DateTime? ProfiledOnUtc { get; set; }
        /// <summary>
        /// The server on which this URL was profiled
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// Was the request a get or a post
        /// </summary>
        public string HttpMethod { get; set; }
        /// <summary>
        /// Is this enabled for profiling?
        /// </summary>
        public bool Enabled { get; set; }

        public ProfiledRequest()
        {
            Id = Guid.NewGuid();
        }
    }
}
