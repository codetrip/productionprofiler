
using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public interface IProfilingCoordinator
    {
        /// <summary>
        /// Tells the profiling coordinator a new request is starting, the coordinator is responsible for determining whether
        /// to profile the current request, this method should return true if we should profile the current request for this coordinator
        /// note that there can be multiple coordinators fora single request, i.e. multiple conditions that trigger a profile to occur
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool ShouldProfile(HttpContext context);
        /// <summary>
        /// Method invoked by the IRequestProfile at the end of a request, allowing the coordinator to 
        /// update the request data with any information required (sessionId, samplingId etc)
        /// </summary>
        /// <param name="data"></param>
        void AugmentProfiledRequestData(ProfiledRequestData data);
    }
}
