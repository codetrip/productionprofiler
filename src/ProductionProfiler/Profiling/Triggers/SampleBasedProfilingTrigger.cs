using System;
using System.Threading;
using System.Web;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Profiling.Triggers
{
    public sealed class SampleBasedProfilingTrigger : ComponentBase, IProfilingTrigger
    {
        private static Timer _periodTimer;
        private static Timer _frequencyTimer;
        private static readonly SampleContext _context;
        private static readonly object _syncLock = new object();

        static SampleBasedProfilingTrigger()
        {
            _context = new SampleContext();
            _periodTimer = new Timer(PeriodTrigger, _context, Period, Period);
            _frequencyTimer = new Timer(FrequencyTrigger, _context, Frequency, Frequency); 
        }

        public bool TriggerProfiling(HttpContext context)
        {
            if (!Enabled)
            {
                Trace("Sample based trigger is disabled, returning false");
                return false;
            }

            return _context.PeriodEnabled;
        }

        public void AugmentProfiledRequestData(ProfiledRequestData data)
        {
            if (_context.FrequencyEnabled)
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

        private static void FrequencyTrigger(object stateInfo)
        {
            lock(_syncLock)
            {
                if (_context != null)
                {
                    _context.DoneForPeriod = false;
                    _context.FrequencyEnabled = !_context.FrequencyEnabled;
                    System.Diagnostics.Trace.Write("FrequencyEnabled:=" + _context.FrequencyEnabled);
                }
            }
        }

        private static void PeriodTrigger(object stateInfo)
        {
            lock (_syncLock)
            {
                if (_context != null)
                {
                    if (_context.FrequencyEnabled)
                    {
                        if (_context.PeriodEnabled || _context.DoneForPeriod)
                        {
                            _context.PeriodEnabled = false;
                            _context.SampleId = Guid.Empty;
                        }
                        else 
                        {
                            _context.PeriodEnabled = true;
                            _context.SampleId = Guid.NewGuid();
                            _context.DoneForPeriod = true;
                        }
                    }

                    System.Diagnostics.Trace.Write("Context.PeriodEnabled = " + _context.PeriodEnabled);
                    System.Diagnostics.Trace.Write("Context.DoneForPeriod = " + _context.DoneForPeriod);
                    System.Diagnostics.Trace.Write("Context.SampleId = " + _context.SampleId);
                }
            }
        }
 
        private class SampleContext
        {
            public Guid SampleId { get; set; }
            public bool FrequencyEnabled { get; set; }
            public bool PeriodEnabled { get; set; }
            public bool DoneForPeriod { get; set; }

            public SampleContext()
            {
                SampleId = Guid.NewGuid();
                FrequencyEnabled = false;
                PeriodEnabled = false;
                DoneForPeriod = false;
            }
        }
    }
}
