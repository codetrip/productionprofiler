
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Logging
{
    /// <summary>
    /// Interface to allow logging events to be captured by the profiler
    /// </summary>
    public interface ILogger : IDoNotWantToBeProfiled
    {
        /// <summary>
        /// Method invoked when we start profiling a request
        /// </summary>
        void Start();
        /// <summary>
        /// Method invoked when we stop profiling a request
        /// </summary>
        void Stop();
        /// <summary>
        /// Static initialisation of the logger if required, called once on application startup
        /// </summary>
		void Initialise();
        /// <summary>
        /// Sets the current MethodData on the logger, log messages should always be appended to this method when they are raised
        /// </summary>
        MethodData CurrentMethod { set; }
    }
}
