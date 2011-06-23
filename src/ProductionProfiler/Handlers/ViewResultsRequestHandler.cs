
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Handlers
{
    public class ViewResultsRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;
        private readonly IProfilerCacheEngine _profilerCacheEngine;

        public ViewResultsRequestHandler(IProfilerRepository repository, IProfilerCacheEngine profilerCacheEngine)
        {
            _repository = repository;
            _profilerCacheEngine = profilerCacheEngine;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (requestInfo.Action == Constants.Actions.Results)
            {
                var data = _profilerCacheEngine.Get(
                    "{0}-{1}-{2}".FormatWith(Constants.Actions.Results, requestInfo.Paging.PageNumber, requestInfo.Paging.PageSize), 
                    () => _repository.GetDistinctProfiledRequestUrls(requestInfo.Paging));

                return new JsonResponse
                {
                    Data = data,
                    Paging = data.Pagination
                };
            }

            if (requestInfo.Action == Constants.Actions.PreviewResults)
            {
                var data = _profilerCacheEngine.Get(
                    "{0}-{1}-{2}-{3}".FormatWith(Constants.Actions.PreviewResults, requestInfo.Url, requestInfo.Paging.PageNumber, requestInfo.Paging.PageSize),
                    () => _repository.GetProfiledRequestDataPreviewByUrl(requestInfo.Url, requestInfo.Paging));

                return new JsonResponse
                {
                    Data = data,
                    Paging = data.Pagination
                };
            }

            if (requestInfo.Action == Constants.Actions.ResultDetails)
            {
                var data = _repository.GetProfiledRequestDataById(requestInfo.Id);

                return new JsonResponse
                {
                    Data = data
                };
            }

            return null;
        }
    }
}
