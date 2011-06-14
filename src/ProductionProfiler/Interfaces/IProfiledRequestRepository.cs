
using System.Collections.Generic;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Interfaces
{
    public interface IProfiledRequestRepository : IRepository<ProfiledRequest, string>
    {
        Page<ProfiledRequest> GetProfiledRequests(PagingInfo pagingInfo);
        IList<ProfiledRequest> GetRequestsToProfile(string serverName);
    }
}
