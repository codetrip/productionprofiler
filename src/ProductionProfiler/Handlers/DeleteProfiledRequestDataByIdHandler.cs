using System;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class DeleteProfiledDataByIdRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _profiledRequestsRepository;

        public DeleteProfiledDataByIdRequestHandler(IProfilerRepository profiledRequestsRepository)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            Guid id;
            if (Guid.TryParse(requestInfo.Form.Get("Id"), out id))
            {
                _profiledRequestsRepository.DeleteProfiledRequestDataById(id);
            }

            return new JsonResponse
            {
                Redirect = string.Format("/profiler?handler={0}&action={1}&url={2}", 
                    Constants.Handlers.Results, 
                    Constants.Actions.PreviewResults,
                    requestInfo.Url)
            };
        }
    }
}
