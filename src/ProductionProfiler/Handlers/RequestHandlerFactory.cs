
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.Handlers
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
