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
        private readonly ICacheEngine _cacheEngine;

        public DeleteProfiledDataByIdRequestHandler(IProfilerRepository profiledRequestsRepository, ICacheEngine cacheEngine)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
            _cacheEngine = cacheEngine;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            Guid id;
            if (Guid.TryParse(requestInfo.Form.Get("Id"), out id))
            {
                _profiledRequestsRepository.DeleteProfiledRequestDataById(id);
                _cacheEngine.Remove(Constants.Actions.Results, true);
                _cacheEngine.Remove("{0}-{1}".FormatWith(Constants.Actions.PreviewResults, requestInfo.Form.Get("Url")), true);
            }

            return new JsonResponse
            {
                Redirect = string.Format("/profiler?handler={0}&action={1}&url={2}", 
                    Constants.Handlers.Results, 
                    Constants.Actions.PreviewResults,
                    requestInfo.Url)
            };
        }
    }
}
