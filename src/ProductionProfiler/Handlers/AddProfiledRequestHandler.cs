
using ProductionProfiler.Core.Binding;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Handlers
{
    public class AddProfiledRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _profiledRequestsRepository;
        private readonly IAddProfiledRequestRequestBinder _addProfiledRequestRequestBinder;
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public AddProfiledRequestHandler(IProfilerRepository profiledRequestsRepository,
            IAddProfiledRequestRequestBinder addProfiledRequestRequestBinder, 
            IProfilerCacheEngine profilerCacheEngine)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
            _profilerCacheEngine = profilerCacheEngine;
            _addProfiledRequestRequestBinder = addProfiledRequestRequestBinder;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (!_addProfiledRequestRequestBinder.IsValid(requestInfo.Form))
            {
                return new JsonResponse
                {
                    Success = false,
                    Errors = _addProfiledRequestRequestBinder.Errors
                };
            }

            var profiledRequest = _addProfiledRequestRequestBinder.Bind(requestInfo.Form);

            //store the new request
            _profiledRequestsRepository.SaveProfiledRequest(profiledRequest);

            //invalidate the current requests to profile cache key
            _profilerCacheEngine.Remove(Constants.CacheKeys.CurrentRequestsToProfile);   

            return new JsonResponse
            {
                Redirect = Constants.Urls.ProfilerHandler.FormatWith(Constants.Handlers.ViewProfiledRequests)
            };
        }
    }
}
