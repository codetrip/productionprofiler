using System.Web;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling.Triggers
{
    public class SampleBasedProfilingTrigger : ComponentBase, IProfilingTrigger
    {
        private readonly ProfilerConfiguration _configuration;

        public SampleBasedProfilingTrigger(ProfilerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool TriggerProfiling(HttpContext context)
        {
            if (!Enabled)
            {
                Trace("Sample based trigger is disabled, returning false");
                return false;
            }

            return false;
        }

        public void AugmentProfiledRequestData(ProfiledRequestData data)
        {

        }

        private bool Enabled
        {
            get
            {
                return _configuration.Settings.ContainsKey(ProfilerConfiguration.SettingKeys.SamplingTriggerEnabled) &&
                       _configuration.Settings[ProfilerConfiguration.SettingKeys.SamplingTriggerEnabled] == "true";
            }
        }
    }
}
