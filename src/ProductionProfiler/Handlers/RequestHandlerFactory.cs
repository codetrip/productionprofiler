
using ProductionProfiler.Core.Handlers.Entities;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Handlers
{
    public static class RequestHandlerFactory
    {
        public static IRequestHandler Create(RequestInfo requestInfo)
        {
            if (!string.IsNullOrEmpty(requestInfo.Handler))
                return ProfilerContext.Current.GetRequestHandler(requestInfo.Handler);

            if (!string.IsNullOrEmpty(requestInfo.ResourceName))
                return new ResourceRequestHandler();

            return ProfilerContext.Current.GetRequestHandler(Constants.Handlers.ViewProfiledRequests);
        }
    }
}
