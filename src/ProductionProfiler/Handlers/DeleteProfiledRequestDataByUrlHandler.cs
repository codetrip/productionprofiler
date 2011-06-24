using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
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
            string url = requestInfo.Form.Get("Url");

            _profiledRequestsRepository.DeleteProfiledRequestDataByUrl(url);
            _profiledRequestsRepository.DeleteResponseByUrl(url);

            return new JsonResponse
            {
                Redirect = string.Format(Constants.Urls.ProfilerHandlerAction, Constants.Handlers.Results, Constants.Actions.Results)
            };
        }
    }
}
