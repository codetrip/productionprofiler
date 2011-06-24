
using System;

namespace ProductionProfiler.Core.Handlers.Entities
{
    [Serializable]
    public class ProfiledRequestUpdateModel
    {
        public string Url { get; set; }
        public int? ProfilingCount { get; set; }
        public string Server { get; set; }
        public bool Delete { get; set; }
        public bool Enabled { get; set; }
    }
}
