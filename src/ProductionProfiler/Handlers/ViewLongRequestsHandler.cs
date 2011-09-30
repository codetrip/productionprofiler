using System.Web;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;

namespace ProductionProfiler.Core.Handlers
{
    public class ViewLongRequestsHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;

        public ViewLongRequestsHandler(IProfilerRepository repository)
        {
            _repository = repository;
        }
        
        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            var longRequests = _repository.GetLongRequests(requestInfo.Paging);

            return new JsonResponse
            {
                Data = longRequests,
                Paging = longRequests.Pagination
            };
        }
    }
}