using ProductionProfiler.Core.Binders;
using ProductionProfiler.Core.Interfaces;
using ProductionProfiler.Core.Interfaces.Entities;
using ProductionProfiler.Core.Interfaces.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public class UpdateProfiledRequestHandler : RequestHandlerBase
    {
        private readonly IProfilerRepository _repository;
        private readonly IUpdateProfiledRequestModelBinder _updateProfiledRequestModelBinder;

        public UpdateProfiledRequestHandler(IProfilerRepository repository, 
            IUpdateProfiledRequestModelBinder updateProfiledRequestModelBinder)
        {
            _repository = repository;
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
                var storedRequest = _repository.GetProfiledRequestByUrl(request.Url);

                if (storedRequest != null)
                    _repository.DeleteProfiledRequest(storedRequest.Url);
            }
            else
            {
                var storedRequest = _repository.GetProfiledRequestByUrl(request.Url);

                if (storedRequest != null)
                {
                    storedRequest.ProfilingCount = request.ProfilingCount.Value;
                    storedRequest.Server = request.Server;
                    storedRequest.Enabled = request.Enabled;

                    _repository.SaveProfiledRequest(storedRequest);
                }
            }

            return new JsonResponse
            {
                Redirect = string.Format("/profiler?handler={0}", Constants.Handlers.ViewProfiledRequests)
            };
        }
    }
}
