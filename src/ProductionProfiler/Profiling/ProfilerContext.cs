
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Factory;
using ProductionProfiler.Core.IoC;

namespace ProductionProfiler.Core.Profiling
{
    public class ProfilerContext
    {
        private const string ProfilingContextKey = "profiling";

        private static bool _initialised;
        private static IContainer _container;
        private static ProfilerConfiguration _configuration;

        internal static void Initialise(IContainer container, ProfilerConfiguration configuration)
        {
            _configuration = configuration;
            _container = container;
            _initialised = true;
        }

        public static ProfilerConfiguration Configuration
        {
            get { return _configuration; }
        }

        public static IContainer Container
        {
            get { return _container; }
        }

        public static bool Initialised
        {
            get { return _initialised; }
        }

        public static bool Profiling
        {
            get
            {
                object val = HttpContextFactory.GetHttpContext().Items[ProfilingContextKey];
                return val != null && (bool) val;
            }
            set
            {
                HttpContextFactory.GetHttpContext().Items[ProfilingContextKey] = value;
            }
        }

        public static IRequestProfiler Profiler
        {
            get { return _container.Resolve<IRequestProfiler>(); }
        }
    }
}