using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;

namespace ProductionProfiler.Core.Handlers
{
    public class ViewUrlToProfilesHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;

        public ViewUrlToProfilesHandler(IProfilerRepository repository)
        {
            _repository = repository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            var UrlToProfiles = _repository.GetUrlsToProfile(requestInfo.Paging);

            return new JsonResponse
            {
                Data = UrlToProfiles,
                Paging = UrlToProfiles.Pagination
            };
        }
    }
}
