using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;

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
