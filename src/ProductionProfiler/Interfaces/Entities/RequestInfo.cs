
using System;

namespace ProductionProfiler.Interfaces.Entities
{
    public class RequestInfo
    {
        public string Handler { get; set; }
        public string Action { get; set; }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public string ResourceName { get; set; }
        public PagingInfo Paging { get; set; }
    }
}
