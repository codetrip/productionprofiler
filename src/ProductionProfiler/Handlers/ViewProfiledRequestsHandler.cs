using ProductionProfiler.Core.Interfaces;
using ProductionProfiler.Core.Interfaces.Entities;

namespace ProductionProfiler.Core.Handlers
{
    public class ViewProfiledRequestsHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;

        public ViewProfiledRequestsHandler(IProfilerRepository repository)
        {
            _repository = repository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            var profiledRequests = _repository.GetProfiledRequests(requestInfo.Paging);

            return new JsonResponse
            {
                Data = profiledRequests,
                Paging = profiledRequests.Pagination
            };
        }
    }
}
