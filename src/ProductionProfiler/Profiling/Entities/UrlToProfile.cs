using System;

namespace ProductionProfiler.Core.Profiling.Entities
{
    [Serializable]
    public class UrlToProfile
    {
        public Guid Id { get; set; }
        /// <summary>
        /// The URL to be monitored
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// The number of times remaining to profile this request
        /// </summary>
        public long? ProfilingCount { get; set; }
        /// <summary>
        /// The server on which this URL was profiled
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// If set, the minimum length of the URL for the info to be persisted.
        /// </summary>
        public int? ThresholdForRecordingMs { get; set; }
        /// <summary>
        /// Is this enabled for profiling?
        /// </summary>
        public bool Enabled { get; set; }

        public UrlToProfile()
        {
            Id = Guid.NewGuid();
        }
    }
}
