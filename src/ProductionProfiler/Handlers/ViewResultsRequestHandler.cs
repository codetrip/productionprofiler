
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class ViewResultsRequestHandler : RequestHandlerBase
    {
        private readonly IProfiledRequestDataRepository _profiledRequestsDataRepository;

        public ViewResultsRequestHandler(IProfiledRequestDataRepository profiledRequestsDataRepository)
        {
            _profiledRequestsDataRepository = profiledRequestsDataRepository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (requestInfo.Action == Constants.Actions.Results)
            {
                var data = _profiledRequestsDataRepository.GetDistinctUrls(requestInfo.Paging);
                return new JsonResponse
                {
                    Data = data,
                    Paging = data.Pagination
                };
            }

            if (requestInfo.Action == Constants.Actions.PreviewResults)
            {
                var data = _profiledRequestsDataRepository.GetPreviewByUrl(requestInfo.Url, requestInfo.Paging);
                return new JsonResponse
                {
                    Data = data,
                    Paging = data.Pagination
                };
            }

            if (requestInfo.Action == Constants.Actions.ResultDetails)
            {
                var data = _profiledRequestsDataRepository.GetById(requestInfo.Id);
                return new JsonResponse
                {
                    Data = data
                };
            }

            return null;
        }
    }
}
