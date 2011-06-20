using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class DeleteProfiledDataByUrlRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _profiledRequestsRepository;
        private readonly ICacheEngine _cacheEngine;

        public DeleteProfiledDataByUrlRequestHandler(IProfilerRepository profiledRequestsRepository, ICacheEngine cacheEngine)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
            _cacheEngine = cacheEngine;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            _profiledRequestsRepository.DeleteProfiledRequestDataByUrl(requestInfo.Form.Get("Url"));
            _cacheEngine.Remove("{0}-{1}".FormatWith(Constants.Actions.PreviewResults, requestInfo.Form.Get("Url")), true);

            return new JsonResponse
            {
                Redirect = string.Format("/profiler?handler={0}&action={1}", Constants.Handlers.Results, Constants.Actions.Results)
            };
        }
    }
}
