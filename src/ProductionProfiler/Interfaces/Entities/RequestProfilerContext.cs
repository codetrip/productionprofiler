using System;
using System.Web;

namespace ProductionProfiler.Interfaces.Entities
{
    public class RequestProfilerContext
    {
        private IContainer _container;
        private Func<HttpRequest, bool> _shouldProfile;
        private bool _monitoringEnabled;
        private static RequestProfilerContext _current = new RequestProfilerContext();

        public static RequestProfilerContext Current
        {
            get
            {
                return _current;
            }
        }

        public bool ShouldProfile(HttpRequest request)
        {
            return _monitoringEnabled && _shouldProfile == null ? false : !request.RawUrl.Contains("/profiler") && _shouldProfile(request);
        }

        public IRequestProfilingCoordinator GetRequestProfilingManager()
        {
            return _container == null ? null : _container.Resolve<IRequestProfilingCoordinator>();
        }

        public IRequestHandler GetRequestHandler(string name)
        {
            return _container == null ? null : _container.Resolve<IRequestHandler>(name);
        }

        internal static void Initialise(Func<HttpRequest, bool> shouldProfileRequest, IContainer container, bool monitoringEnabled)
        {
            _current = new RequestProfilerContext
            {
                _container = container,
                _shouldProfile = shouldProfileRequest,
                _monitoringEnabled = monitoringEnabled
            };
        }

        internal RequestProfilerContext()
        {}
    }
}