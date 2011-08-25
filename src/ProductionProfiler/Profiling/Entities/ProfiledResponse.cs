
using System;

namespace ProductionProfiler.Core.Profiling.Entities
{
    [Serializable]
    public class ProfiledResponse : IAsyncPersistable
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
    }
}
