
using System.Web;
using ProductionProfiler.Core.Modules;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling
{
    public interface IProfilingTrigger
    {
        /// <summary>
        /// Tells the profiling trigger a new request is starting, the trigger is responsible for determining whether
        /// to profile the current request, this method should return true if we should profile the current request for this trigger
        /// note that there can be multiple IProfilingTrigger for a single request, i.e. multiple conditions that trigger a profile to occur
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool TriggerProfiling(HttpContext context);
        /// <summary>
        /// Method invoked by the IRequestProfile at the end of a request, allowing the coordinator to 
        /// update the request data with any information required (sessionId, samplingId etc)
        /// </summary>
        /// <param name="data"></param>
        void AugmentProfiledRequestData(ProfiledRequestData data);
        /// <summary>
        /// After doing all the profiling, has this trigger decided it wants to veto persistence for this request?
        /// </summary>
        bool VetoPersistence(RequestProfileContext context);
    }
}
