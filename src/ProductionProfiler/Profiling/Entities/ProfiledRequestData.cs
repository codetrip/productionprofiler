using System;
using System.Collections.Generic;

namespace ProductionProfiler.Core.Profiling.Entities
{
    [Serializable]
    public class ProfiledRequestData
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string SessionUserId { get; set; }
        public Guid SamplingId { get; set; }
        public DateTime CapturedOnUtc { get; set; }
        public string Url { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string Server { get; set; }
        public string ClientIpAddress { get; set; }
        public bool Ajax { get; set; }
        public bool CapturedResponse { get; set; }
        public List<MethodData> Methods { get; set; }
        public List<DataCollection> RequestData { get; set; }
        public List<DataCollection> ResponseData { get; set; }

        public ProfiledRequestData()
        {
            RequestData = new List<DataCollection>();
            ResponseData = new List<DataCollection>();
            Methods = new List<MethodData>();
        }
    }

    [Serializable]
    public class EncodedProfiledRequestData : ProfiledRequestData
    {
        public string EncodedUrl { get; set; }
    }
}
