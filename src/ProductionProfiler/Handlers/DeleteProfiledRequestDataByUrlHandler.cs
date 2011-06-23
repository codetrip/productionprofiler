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
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public DeleteProfiledDataByUrlRequestHandler(IProfilerRepository profiledRequestsRepository, IProfilerCacheEngine profilerCacheEngine)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
            _profilerCacheEngine = profilerCacheEngine;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            string url = requestInfo.Form.Get("Url");

            _profiledRequestsRepository.DeleteProfiledRequestDataByUrl(url);
            _profiledRequestsRepository.DeleteResponseByUrl(url);

            _profilerCacheEngine.Remove("{0}".FormatWith(Constants.Actions.Results), true);
            _profilerCacheEngine.Remove("{0}-{1}".FormatWith(Constants.Actions.PreviewResults, url), true);

            return new JsonResponse
            {
                Redirect = string.Format(Constants.Urls.ProfilerHandlerAction, Constants.Handlers.Results, Constants.Actions.Results)
            };
        }
    }
}
