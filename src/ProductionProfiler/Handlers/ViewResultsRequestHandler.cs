using ProductionProfiler.Core.Dynamic;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;
using System.Linq;
using RequestInfo = ProductionProfiler.Core.Handlers.Entities.RequestInfo;

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
                        var data = _repository.GetDistinctUrlsToProfile(requestInfo.Paging);

                        return new JsonResponse
                        {
                            Data = data.Select(item => new { Url = item, EncodedUrl = item.Encode() }),
                            Paging = data.Pagination
                        };
                    }
                case Constants.Actions.PreviewResults:
                    {
                        var data = _repository.GetProfiledRequestDataPreviewByUrl(requestInfo.Url.Decode(), requestInfo.Paging);

                        return new JsonResponse
                        {
                            Data = data.Select(item =>
                            {
                                EncodedProfiledRequestDataPreview encodedItem = new EncodedProfiledRequestDataPreview
                                {
                                    EncodedUrl = item.Url.Encode()
                                };
                                PropertyMapper.Map(item, encodedItem);
                                return encodedItem;
                            }),
                            Paging = data.Pagination
                        };
                    }
                case Constants.Actions.ResultDetails:
                    {
                        var data = _repository.GetProfiledRequestDataById(requestInfo.Id);

                        EncodedProfiledRequestData encodedItem = new EncodedProfiledRequestData
                        {
                            EncodedUrl = data.Url.Encode()
                        };

                        PropertyMapper.Map(data, encodedItem);

                        return new JsonResponse
                        {
                            Data = encodedItem
                        };
                    }
                default:
                    return null;
            }
        }
    }
}
