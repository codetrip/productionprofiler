﻿using ProductionProfiler.Binders;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class AddProfiledRequestHandler : RequestHandlerBase
    {
        private readonly IProfiledRequestRepository _profiledRequestsRepository;
        private readonly IAddProfiledRequestModelBinder _addProfiledRequestModelBinder;

        public AddProfiledRequestHandler(IProfiledRequestRepository profiledRequestsRepository,
            IAddProfiledRequestModelBinder addProfiledRequestModelBinder)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
            _addProfiledRequestModelBinder = addProfiledRequestModelBinder;
        }

        protected override JsonResponse DoHandleRequest(RequestInfo requestInfo)
        {
            if (!_addProfiledRequestModelBinder.IsValid(requestInfo.Form))
            {
                return new JsonResponse
                {
                    Success = false,
                    Errors = _addProfiledRequestModelBinder.Errors
                };
            }

            _profiledRequestsRepository.Save(_addProfiledRequestModelBinder.Bind(requestInfo.Form));

            return new JsonResponse
            {
                Redirect = string.Format("/profiler?handler={0}", Constants.Handlers.ViewProfiledRequests)
            };
        }
    }
}
