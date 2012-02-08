
using System;

namespace ProductionProfiler.Core.Handlers.Entities
{
    [Serializable]
    public class UrlToProfileUpdateModel
    {
        public string Url { get; set; }
        public int? ProfilingCount { get; set; }
        public string Server { get; set; }
        public bool Delete { get; set; }
        public bool Enabled { get; set; }
        public int? ThresholdForRecordingMs { get; set; }
    }
}
