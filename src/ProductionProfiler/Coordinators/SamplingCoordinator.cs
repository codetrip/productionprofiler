using System.Web;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Coordinators
{
    public class SamplingCoordinator : ComponentBase, IProfilingCoordinator
    {
        public bool ShouldProfile(HttpContext context)
        {
            return false;
        }

        public void AugmentProfiledRequestData(ProfiledRequestData data)
        {

        }
    }
}
