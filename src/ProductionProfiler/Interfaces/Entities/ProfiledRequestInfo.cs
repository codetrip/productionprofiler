using System;
using System.Collections.Generic;

namespace ProductionProfiler.Interfaces.Entities
{
    [Serializable]
    public class ProfiledRequestInfo
    {
        public DateTime CapturedOnUtc { get; set; }
        public string Url { get; set; }
        public List<ProfiledMethodInfo> Methods { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string Server { get; set; }
        public string ClientIpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool Ajax { get; set; }
        public Guid RequestId { get; set; }
    }
}
