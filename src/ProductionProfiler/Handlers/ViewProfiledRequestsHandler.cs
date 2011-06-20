using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class ViewProfiledRequestsHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;
        private readonly ICacheEngine _cacheEngine;

        public ViewProfiledRequestsHandler(IProfilerRepository repository, ICacheEngine cacheEngine)
        {
            _repository = repository;
            _cacheEngine = cacheEngine;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            var profiledRequests = _cacheEngine.Get("{0}-{1}-{2}".FormatWith(
                Constants.Handlers.ViewProfiledRequests, 
                requestInfo.Paging.PageNumber, 
                requestInfo.Paging.PageSize), () => _repository.GetProfiledRequests(requestInfo.Paging));

            return new JsonResponse
            {
                Data = profiledRequests,
                Paging = profiledRequests.Pagination
            };
        }
    }
}
