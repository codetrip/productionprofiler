using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.Windsor;
using ProductionProfiler.Profiling;

namespace ProductionProfiler.Interfaces.Entities
{
    public class RequestProfilerContext
    {
        private IList<Type> _typesToIntercept;
        private IWindsorContainer _container;
        private Func<HttpRequest, bool> _shouldProfile;
        private static RequestProfilerContext _current;

        public static RequestProfilerContext Current
        {
            get
            {
                if (_current == null)
                    throw new ArgumentException("RequestProfilerContext has not been initialised correctly, please invoke ProfilerConfiguration.With(typesToIntercept).Initialise(IWindsorContainer) at a minimum.");

                return _current;
            }
        }

        public bool ShouldIntercept(Type componentType)
        {
            return _typesToIntercept.Any(t => t.IsAssignableFrom(componentType));
        }

        public bool ShouldProfile(HttpRequest request)
        {
            return _shouldProfile(request);
        }

        public IRequestProfilingManager GetRequestProfilingManager()
        {
            return _container.Resolve<IRequestProfilingManager>();
        }

        internal static void Initialise(Func<HttpRequest, bool> shouldProfileRequest, IList<Type> typesToIntercept, IWindsorContainer container)
        {
            _current = new RequestProfilerContext
                           {
                               _typesToIntercept = typesToIntercept,
                               _container = container,
                               _shouldProfile = shouldProfileRequest
                           };
        }

        internal RequestProfilerContext()
        {}
    }
}