using System;
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class DeleteProfiledDataByIdRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _urlToProfileRepository;

        public DeleteProfiledDataByIdRequestHandler(IProfilerRepository urlToProfileRepository)
        {
            _urlToProfileRepository = urlToProfileRepository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            Guid id = default(Guid);

            try
            {
                id = new Guid(requestInfo.Form.Get("Id"));
            }
            catch (Exception)
            {}

            if (id != default(Guid))
            {
                _urlToProfileRepository.DeleteProfiledRequestDataById(id);
                _urlToProfileRepository.DeleteResponseById(id);
            }

            return new JsonResponse
            {
                Redirect = string.Format(Constants.Urls.ProfilerHandlerActionUrl, 
                    Constants.Handlers.Results, 
                    Constants.Actions.PreviewResults,
                    requestInfo.Url)
            };
        }
    }
}
