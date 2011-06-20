
using System;
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
        private readonly ICacheEngine _cacheEngine;

        public ViewResultsRequestHandler(IProfilerRepository repository, ICacheEngine cacheEngine)
        {
            _repository = repository;
            _cacheEngine = cacheEngine;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (requestInfo.Action == Constants.Actions.Results)
            {
                var data = _cacheEngine.Get(
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
                var data = _cacheEngine.Get(
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
                var data = _cacheEngine.Get(requestInfo.Id.ToString(), () => _repository.GetProfiledRequestDataById(requestInfo.Id), true, new TimeSpan(1, 0, 0));

                return new JsonResponse
                {
                    Data = data
                };
            }

            return null;
        }
    }
}
