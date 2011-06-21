
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
        private readonly ICacheEngine _cacheEngine;

        public AddProfiledRequestHandler(IProfilerRepository profiledRequestsRepository,
            IAddProfiledRequestRequestBinder addProfiledRequestRequestBinder, 
            ICacheEngine cacheEngine)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
            _cacheEngine = cacheEngine;
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

            _profiledRequestsRepository.SaveProfiledRequest(_addProfiledRequestRequestBinder.Bind(requestInfo.Form));
            _cacheEngine.Remove(Constants.Handlers.ViewProfiledRequests, true);

            return new JsonResponse
            {
                Redirect = Constants.Urls.ProfilerHandler.FormatWith(Constants.Handlers.ViewProfiledRequests)
            };
        }
    }
}
