
using ProductionProfiler.Core.Interfaces;
using ProductionProfiler.Core.Interfaces.Entities;
using ProductionProfiler.Core.Interfaces.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class ViewResultsRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;

        public ViewResultsRequestHandler(IProfilerRepository repository)
        {
            _repository = repository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (requestInfo.Action == Constants.Actions.Results)
            {
                var data = _repository.GetDistinctProfiledRequestUrls(requestInfo.Paging);
                return new JsonResponse
                {
                    Data = data,
                    Paging = data.Pagination
                };
            }

            if (requestInfo.Action == Constants.Actions.PreviewResults)
            {
                var data = _repository.GetProfiledRequestDataPreviewByUrl(requestInfo.Url, requestInfo.Paging);
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
