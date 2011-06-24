using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;

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
            switch(requestInfo.Action)
            {
                case Constants.Actions.Results:
                    {
                        var data = _repository.GetDistinctProfiledRequestUrls(requestInfo.Paging);

                        return new JsonResponse
                        {
                            Data = data,
                            Paging = data.Pagination
                        };
                    }
                case Constants.Actions.PreviewResults:
                    {
                        var data = _repository.GetProfiledRequestDataPreviewByUrl(requestInfo.Url, requestInfo.Paging);

                        return new JsonResponse
                        {
                            Data = data,
                            Paging = data.Pagination
                        };
                    }
                case Constants.Actions.ResultDetails:
                    {
                        var data = _repository.GetProfiledRequestDataById(requestInfo.Id);

                        return new JsonResponse
                        {
                            Data = data
                        };
                    }
                default:
                    return null;
            }
        }
    }
}
