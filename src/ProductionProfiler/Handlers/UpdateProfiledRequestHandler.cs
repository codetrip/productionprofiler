using ProductionProfiler.Core.Binding;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class UpdateUrlToProfileHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;
        private readonly IUpdateUrlToProfileRequestBinder _updateUrlToProfileRequestBinder;
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public UpdateUrlToProfileHandler(IProfilerRepository repository, 
            IUpdateUrlToProfileRequestBinder updateUrlToProfileRequestBinder, 
            IProfilerCacheEngine profilerCacheEngine)
        {
            _repository = repository;
            _profilerCacheEngine = profilerCacheEngine;
            _updateUrlToProfileRequestBinder = updateUrlToProfileRequestBinder;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (!_updateUrlToProfileRequestBinder.IsValid(requestInfo.Form))
            {
                return new JsonResponse
                {
                    Success = false,
                    Errors = _updateUrlToProfileRequestBinder.Errors
                };
            }

            var request = _updateUrlToProfileRequestBinder.Bind(requestInfo.Form);

            if (request.Delete)
            {
                var storedRequest = _repository.GetUrlToProfile(request.Url);

                if (storedRequest != null)
                    _repository.DeleteUrlToProfile(storedRequest.Url);
            }
            else
            {
                var storedRequest = _repository.GetUrlToProfile(request.Url);

                if (storedRequest != null)
                {
                    storedRequest.ProfilingCount = request.ProfilingCount.Value;
                    storedRequest.Server = request.Server;
                    storedRequest.Enabled = request.Enabled;

                    _repository.SaveUrlToProfile(storedRequest);
                }
            }

            //ensure we remove the current set of requests to profile form the cache
            _profilerCacheEngine.Remove(Constants.CacheKeys.CurrentRequestsToProfile);

            return new JsonResponse
            {
                Redirect = string.Format(Constants.Urls.ProfilerHandler, Constants.Handlers.ViewUrlToProfiles)
            };
        }
    }
}
