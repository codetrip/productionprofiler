
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Configuration
{
    public interface IFluentCoordinatorConfiguration
    {
        IFluentConfiguration Register<T>() where T : IProfilingCoordinator;
        IFluentCoordinatorConfiguration Url();
        IFluentCoordinatorConfiguration Session();
        IFluentCoordinatorConfiguration Sampling();
        IFluentConfiguration Add();
    }
}