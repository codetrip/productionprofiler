using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class ProfiledRequestsRequestHandler : RequestHandlerBase
    {
        private readonly IProfiledRequestRepository _profiledRequestsRepository;

        public ProfiledRequestsRequestHandler(IProfiledRequestRepository profiledRequestsRepository)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
        }

        protected override object GetResponseData(RequestInfo requestInfo)
        {
            return _profiledRequestsRepository.GetProfiledRequests(requestInfo.Paging);
        }

        protected override string Action(RequestInfo requestInfo)
        {
            return Constants.Actions.ViewProfiledRequests;
        }
    }
}
