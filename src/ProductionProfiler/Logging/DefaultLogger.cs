using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Core.Logging
{
    public class DefaultLogger : ILogger
    {
        public void StartProfiling()
        { }

        public void StopProfiling()
        { }

        public void Initialise()
        { }

        public MethodData CurrentMethod
        {
            set {  }
        }
    }
}
