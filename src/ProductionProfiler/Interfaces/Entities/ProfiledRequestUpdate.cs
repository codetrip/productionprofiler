
using System;

namespace ProductionProfiler.Interfaces.Entities
{
    [Serializable]
    public class ProfiledRequestUpdate
    {
        public string Url { get; set; }
        public int? ProfilingCount { get; set; }
        public string Server { get; set; }
        public bool Ignore { get; set; }
    }
}
