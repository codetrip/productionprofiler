using System.Web;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling.Triggers
{
    public class SampleBasedProfilingTrigger : ComponentBase, IProfilingTrigger
    {

        public bool TriggerProfiling(HttpContext context)
        {
            return false;
        }

        public void AugmentProfiledRequestData(ProfiledRequestData data)
        {

        }
    }
}
