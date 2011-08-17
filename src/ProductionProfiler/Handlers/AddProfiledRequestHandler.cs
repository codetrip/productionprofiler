
using ProductionProfiler.Core.Binding;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Handlers
{
    public class AddUrlToProfileHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _UrlToProfilesRepository;
        private readonly IAddUrlToProfileRequestBinder _addUrlToProfileRequestBinder;
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public AddUrlToProfileHandler(IProfilerRepository UrlToProfilesRepository,
            IAddUrlToProfileRequestBinder addUrlToProfileRequestBinder, 
            IProfilerCacheEngine profilerCacheEngine)
        {
            _UrlToProfilesRepository = UrlToProfilesRepository;
            _profilerCacheEngine = profilerCacheEngine;
            _addUrlToProfileRequestBinder = addUrlToProfileRequestBinder;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (!_addUrlToProfileRequestBinder.IsValid(requestInfo.Form))
            {
                return new JsonResponse
                {
                    Success = false,
                    Errors = _addUrlToProfileRequestBinder.Errors
                };
            }

            var UrlToProfile = _addUrlToProfileRequestBinder.Bind(requestInfo.Form);

            //store the new request
            _UrlToProfilesRepository.SaveUrlToProfile(UrlToProfile);

            //invalidate the current requests to profile cache key
            _profilerCacheEngine.Remove(Constants.CacheKeys.CurrentRequestsToProfile);   

            return new JsonResponse
            {
                Redirect = Constants.Urls.ProfilerHandler.FormatWith(Constants.Handlers.ViewUrlToProfiles)
            };
        }
    }
}
