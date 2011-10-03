using System.Web;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;

namespace ProductionProfiler.Core.Handlers
{
    public class ClearLongRequestsHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;

        public ClearLongRequestsHandler(IProfilerRepository repository)
        {
            _repository = repository;
        }
        
        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            _repository.DeleteAllTimedRequests();

            return new JsonResponse
            {
                Redirect = "/profiler?handler=longrequests&action=viewlongrequests"
            };
        }
    }
}