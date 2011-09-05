
using System;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Configuration
{
    public interface IFluentProfilingTriggerConfiguration
    {
        IFluentConfiguration ByCustomTrigger<T>() where T : IProfilingTrigger;
        IFluentConfiguration ByUrl();
        IFluentConfiguration BySession();
        IFluentSampleProfilingTriggerConfiguration BySampling();
    }

    public interface IFluentSampleProfilingTriggerConfiguration
    {
        IFluentSampleProfilingTriggerConfiguration Every(TimeSpan frequency);
        IFluentSampleProfilingTriggerConfiguration For(TimeSpan period);
        IFluentConfiguration Enable();
    }
}