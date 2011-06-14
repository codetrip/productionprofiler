using System;
using System.Web;
using Castle.Windsor;

namespace ProductionProfiler.Interfaces.Entities
{
    public class RequestProfilerContext
    {
        private IWindsorContainer _container;
        private Func<HttpRequest, bool> _shouldProfile;
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
            return _shouldProfile == null ? false : _shouldProfile(request);
        }

        public IRequestProfilingCoordinator GetRequestProfilingManager()
        {
            return _container == null ? null : _container.Resolve<IRequestProfilingCoordinator>();
        }

        public IRequestHandler GetRequestHandler(string name)
        {
            return _container == null ? null : _container.Resolve<IRequestHandler>(name);
        }

        internal static void Initialise(Func<HttpRequest, bool> shouldProfileRequest, IWindsorContainer container)
        {
            _current = new RequestProfilerContext
            {
                _container = container,
                _shouldProfile = shouldProfileRequest
            };
        }

        internal RequestProfilerContext()
        {}
    }
}