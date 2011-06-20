
using ProductionProfiler.Core.Interfaces;
using ProductionProfiler.Core.Interfaces.Entities;
using ProductionProfiler.Core.Interfaces.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public static class RequestHandlerFactory
    {
        public static IRequestHandler Create(RequestInfo requestInfo)
        {
            if (!string.IsNullOrEmpty(requestInfo.Handler))
                return RequestProfilerContext.Current.GetRequestHandler(requestInfo.Handler);

            if (!string.IsNullOrEmpty(requestInfo.ResourceName))
                return new ResourceRequestHandler();

            return RequestProfilerContext.Current.GetRequestHandler(Constants.Handlers.ViewProfiledRequests);
        }
    }
}
