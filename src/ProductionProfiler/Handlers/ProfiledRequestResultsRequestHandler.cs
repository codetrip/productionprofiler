
using System;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
{
    public class ProfiledRequestResultsRequestHandler : RequestHandlerBase
    {
        private readonly IProfiledRequestDataRepository _profiledRequestsDataRepository;

        public ProfiledRequestResultsRequestHandler(IProfiledRequestDataRepository profiledRequestsDataRepository)
        {
            _profiledRequestsDataRepository = profiledRequestsDataRepository;
        }

        protected override object GetResponseData(RequestInfo requestInfo)
        {
            if (requestInfo.Action == Constants.Actions.Results)
                return _profiledRequestsDataRepository.GetDistinctUrls(requestInfo.Paging);

            if (requestInfo.Action == Constants.Actions.PreviewResults)
                return _profiledRequestsDataRepository.GetPreviewByUrl(requestInfo.Url, requestInfo.Paging);

            if (requestInfo.Action == Constants.Actions.ResultDetails)
                return _profiledRequestsDataRepository.GetById(requestInfo.Id);

            return null;
        }

        protected override string Action(RequestInfo requestInfo)
        {
            return requestInfo.Action;
        }
    }
}
