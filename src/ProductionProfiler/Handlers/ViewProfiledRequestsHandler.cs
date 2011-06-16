using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;

namespace ProductionProfiler.Handlers
{
    public class ViewProfiledRequestsHandler : RequestHandlerBase
    {
        private readonly IProfiledRequestRepository _profiledRequestsRepository;

        public ViewProfiledRequestsHandler(IProfiledRequestRepository profiledRequestsRepository)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            var profiledRequests = _profiledRequestsRepository.GetProfiledRequests(requestInfo.Paging);

            return new JsonResponse
            {
                Data = profiledRequests,
                Paging = profiledRequests.Pagination
            };
        }
    }
}
