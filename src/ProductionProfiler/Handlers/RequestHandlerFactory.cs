
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
                return ProfilerContext.Container.Resolve<IRequestHandler>(requestInfo.Handler);

            if (!string.IsNullOrEmpty(requestInfo.ResourceName))
                return new ResourceRequestHandler();

            return ProfilerContext.Container.Resolve<IRequestHandler>(Constants.Handlers.ViewUrlToProfiles);
        }
    }
}
