using ProductionProfiler.Binders;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class UpdateProfiledRequestHandler : RequestHandlerBase
    {
        private readonly IProfiledRequestRepository _profiledRequestsRepository;
        private readonly IUpdateProfiledRequestModelBinder _updateProfiledRequestModelBinder;

        public UpdateProfiledRequestHandler(IProfiledRequestRepository profiledRequestsRepository, 
            IUpdateProfiledRequestModelBinder updateProfiledRequestModelBinder)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
            _updateProfiledRequestModelBinder = updateProfiledRequestModelBinder;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (!_updateProfiledRequestModelBinder.IsValid(requestInfo.Form))
            {
                return new JsonResponse
                {
                    Success = false,
                    Errors = _updateProfiledRequestModelBinder.Errors
                };
            }

            var request = _updateProfiledRequestModelBinder.Bind(requestInfo.Form);

            if (request.Delete)
            {
                var storedRequest = _profiledRequestsRepository.GetById(request.Url);

                if (storedRequest != null)
                    _profiledRequestsRepository.Delete(new { _id = storedRequest.Url });
            }
            else
            {
                var storedRequest = _profiledRequestsRepository.GetById(request.Url);

                if (storedRequest != null)
                {
                    storedRequest.ProfilingCount = request.ProfilingCount.Value;
                    storedRequest.Server = request.Server;
                    storedRequest.Enabled = request.Enabled;

                    _profiledRequestsRepository.Save(storedRequest);
                }
            }

            return new JsonResponse
            {
                Redirect = string.Format("/profiler?handler={0}", Constants.Handlers.ViewProfiledRequests)
            };
        }
    }
}
