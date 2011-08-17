using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class DeleteProfiledDataByUrlRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _urlToProfilesRepository;

        public DeleteProfiledDataByUrlRequestHandler(IProfilerRepository urlToProfilesRepository)
        {
            _urlToProfilesRepository = urlToProfilesRepository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            string url = requestInfo.Form.Get("Url");

            _urlToProfilesRepository.DeleteProfiledRequestDataByUrl(url);
            _urlToProfilesRepository.DeleteResponseByUrl(url);

            return new JsonResponse
            {
                Redirect = string.Format(Constants.Urls.ProfilerHandlerAction, Constants.Handlers.Results, Constants.Actions.Results)
            };
        }
    }
}
