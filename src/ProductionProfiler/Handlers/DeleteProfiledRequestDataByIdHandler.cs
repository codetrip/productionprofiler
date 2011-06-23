using System;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class DeleteProfiledDataByIdRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _profiledRequestsRepository;
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public DeleteProfiledDataByIdRequestHandler(IProfilerRepository profiledRequestsRepository, IProfilerCacheEngine profilerCacheEngine)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
            _profilerCacheEngine = profilerCacheEngine;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            Guid id;
            if (Guid.TryParse(requestInfo.Form.Get("Id"), out id))
            {
                _profiledRequestsRepository.DeleteProfiledRequestDataById(id);
                _profiledRequestsRepository.DeleteResponseById(id);
                _profilerCacheEngine.Remove(Constants.Actions.Results, true);
                _profilerCacheEngine.Remove("{0}-{1}".FormatWith(Constants.Actions.PreviewResults, requestInfo.Form.Get("Url")), true);
            }

            return new JsonResponse
            {
                Redirect = string.Format(Constants.Urls.ProfilerHandlerActionUrl, 
                    Constants.Handlers.Results, 
                    Constants.Actions.PreviewResults,
                    requestInfo.Url)
            };
        }
    }
}
