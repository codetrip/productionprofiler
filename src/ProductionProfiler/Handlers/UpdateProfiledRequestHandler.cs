using ProductionProfiler.Core.Binding;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class UpdateProfiledRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;
        private readonly IUpdateProfiledRequestRequestBinder _updateProfiledRequestRequestBinder;
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public UpdateProfiledRequestHandler(IProfilerRepository repository, 
            IUpdateProfiledRequestRequestBinder updateProfiledRequestRequestBinder, 
            IProfilerCacheEngine profilerCacheEngine)
        {
            _repository = repository;
            _profilerCacheEngine = profilerCacheEngine;
            _updateProfiledRequestRequestBinder = updateProfiledRequestRequestBinder;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (!_updateProfiledRequestRequestBinder.IsValid(requestInfo.Form))
            {
                return new JsonResponse
                {
                    Success = false,
                    Errors = _updateProfiledRequestRequestBinder.Errors
                };
            }

            var request = _updateProfiledRequestRequestBinder.Bind(requestInfo.Form);

            if (request.Delete)
            {
                var storedRequest = _repository.GetProfiledRequestByUrl(request.Url);

                if (storedRequest != null)
                    _repository.DeleteProfiledRequest(storedRequest.Url);
            }
            else
            {
                var storedRequest = _repository.GetProfiledRequestByUrl(request.Url);

                if (storedRequest != null)
                {
                    storedRequest.ProfilingCount = request.ProfilingCount.Value;
                    storedRequest.Server = request.Server;
                    storedRequest.Enabled = request.Enabled;

                    _repository.SaveProfiledRequest(storedRequest);
                }
            }

            _profilerCacheEngine.Remove(Constants.Handlers.ViewProfiledRequests, true);

            return new JsonResponse
            {
                Redirect = string.Format(Constants.Urls.ProfilerHandler, Constants.Handlers.ViewProfiledRequests)
            };
        }
    }
}
