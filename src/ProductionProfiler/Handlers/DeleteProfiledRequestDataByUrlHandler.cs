using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class DeleteProfiledDataByUrlRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _profiledRequestsRepository;

        public DeleteProfiledDataByUrlRequestHandler(IProfilerRepository profiledRequestsRepository)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            _profiledRequestsRepository.DeleteProfiledRequestDataByUrl(requestInfo.Form.Get("Url"));

            return new JsonResponse
            {
                Redirect = string.Format("/profiler?handler={0}&action={1}", Constants.Handlers.Results, Constants.Actions.Results)
            };
        }
    }
}
