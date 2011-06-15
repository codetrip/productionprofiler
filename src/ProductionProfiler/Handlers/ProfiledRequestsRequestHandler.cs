using ProductionProfiler.Binders;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class ProfiledRequestsRequestHandler : RequestHandlerBase
    {
        private readonly IProfiledRequestRepository _profiledRequestsRepository;

        public ProfiledRequestsRequestHandler(IProfiledRequestRepository profiledRequestsRepository)
        {
            _profiledRequestsRepository = profiledRequestsRepository;
        }

        protected override JsonResponse GetResponseData(RequestInfo requestInfo)
        {
            switch(requestInfo.Action)
            {
                case Constants.Actions.Add:
                    {
                        _profiledRequestsRepository.Save(new AddProfiledRequestModelBinder().Bind(requestInfo.Form));

                        return new JsonResponse
                        {
                            Redirect = string.Format("/profiler?handler={0}&action={1}", Constants.Handlers.ProfiledRequests, Constants.Actions.ViewProfiledRequests)
                        };
                    }
                case Constants.Actions.Update:
                    {
                        var requests = new UpdateProfiledRequestModelBinder().Bind(requestInfo.Form);

                        foreach (var url in requests)
                        {
                            if (url.Ignore)
                            {
                                var storedUrl = _profiledRequestsRepository.GetById(url.Url);

                                if (storedUrl != null)
                                    _profiledRequestsRepository.Delete(new {_id = storedUrl.Url });
                            }
                            else if (url.ProfilingCount.HasValue)
                            {
                                var storedUrl = _profiledRequestsRepository.GetById(url.Url);
                                storedUrl.ProfilingCount = url.ProfilingCount.Value;
                                storedUrl.Server = url.Server;

                                _profiledRequestsRepository.Save(storedUrl);
                            }
                        }

                        return new JsonResponse
                        {
                            Redirect = string.Format("/profiler?handler={0}&action={1}", Constants.Handlers.ProfiledRequests, Constants.Actions.ViewProfiledRequests)
                        };
                    }
                case Constants.Actions.ViewProfiledRequests:
                    {
                        var data = _profiledRequestsRepository.GetProfiledRequests(requestInfo.Paging);

                        return new JsonResponse
                        {
                            Data = data,
                            Paging = data.Pagination
                        };
                    }
                default:
                    return null;
            }
        }

        protected override string Action(RequestInfo requestInfo)
        {
            return Constants.Actions.ViewProfiledRequests;
        }
    }
}
