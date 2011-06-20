
using System;

namespace ProductionProfiler.Core.Interfaces.Entities
{
    [Serializable]
    public class ProfiledRequestUpdate
    {
        public string Url { get; set; }
        public int? ProfilingCount { get; set; }
        public string Server { get; set; }
        public bool Delete { get; set; }
        public bool Enabled { get; set; }
    }
}
