using System;
using System.Threading;
using System.Web;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling.Triggers
{
    public sealed class SampleBasedProfilingTrigger : ComponentBase, IProfilingTrigger
    {
        private static Timer _timer;
        private static readonly SampleContext _context;
        private static readonly object _syncLock = new object();

        static SampleBasedProfilingTrigger()
        {
            _context = new SampleContext();
            _timer = new Timer(EnableOrDisableSampling, _context, Frequency, Period);
        }

        public bool TriggerProfiling(HttpContext context)
        {
            if (!Enabled)
            {
                Trace("Sample based trigger is disabled, returning false");
                return false;
            }

            return _context.Enabled;
        }

        public void AugmentProfiledRequestData(ProfiledRequestData data)
        {
            if (_context.Enabled)
                data.SamplingId = _context.SampleId;
        }

        private bool Enabled
        {
            get
            {
                return ProfilerContext.Configuration.Settings.ContainsKey(ProfilerConfiguration.SettingKeys.SamplingTriggerEnabled) &&
                       ProfilerContext.Configuration.Settings[ProfilerConfiguration.SettingKeys.SamplingTriggerEnabled] == "true";
            }
        }

        private static TimeSpan Period
        {
            get
            {
                return TimeSpan.Parse(ProfilerContext.Configuration.Settings[ProfilerConfiguration.SettingKeys.SamplingPeriod]);
            }
        }

        private static TimeSpan Frequency
        {
            get
            {
                return TimeSpan.Parse(ProfilerContext.Configuration.Settings[ProfilerConfiguration.SettingKeys.SamplingFrequency]);
            }
        }

        private static void EnableOrDisableSampling(object stateInfo)
        {
            lock(_syncLock)
            {
                System.Diagnostics.Trace.Write("EnableOrDisableSampling Invoked");

                if (_context != null)
                {
                    System.Diagnostics.Trace.Write("Context.Enabled = " + _context.Enabled);
                    System.Diagnostics.Trace.Write("Context.SampleId = " + _context.SampleId);

                    if (_context.Enabled)
                    {
                        _context.Enabled = false;
                        _context.SampleId = Guid.Empty;
                    }
                    else
                    {
                        _context.Enabled = true;
                        _context.SampleId = Guid.NewGuid();
                    }
                }
            }
        }
 
        private class SampleContext
        {
            public Guid SampleId { get; set; }
            public bool Enabled { get; set; }

            public SampleContext()
            {
                SampleId = Guid.NewGuid();
                Enabled = false;
            }
        }
    }
}
