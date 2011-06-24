using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using ProductionProfiler.Core.Handlers;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Profiling
{
    public class RequestProfilerContext
    {
        private IContainer _container;
        private Func<HttpRequest, bool> _shouldProfile;
        private static RequestProfilerContext _current = new RequestProfilerContext();
        private List<ProfilerError> _persistentProfilerErrors;
        private Func<HttpContext, bool> _authorizedForManagement;

        public static RequestProfilerContext Current
        {
            get
            {
                return _current;
            }
        }

        public List<ProfilerError> PersistentProfilerErrors
        {
            get
            {
                return _persistentProfilerErrors;
            }
        }

        public bool ShouldProfile(HttpRequest request)
        {
            return _shouldProfile == null ? false : !request.RawUrl.Contains("/profiler") && _shouldProfile(request);
        }

        public void StartProfiling()
        {
            HttpContext.Current.Items[Constants.RequestProfileContextKey] = true;
        }

        public void StopProfiling()
        {
            HttpContext.Current.Items.Remove(Constants.RequestProfileContextKey);
        }

        public bool ProfilingCurrentRequest()
        {
            return HttpContext.Current.Items.Contains(Constants.RequestProfileContextKey);
        }

        public IContainer Container
        {
            get { return _container; }
        }

        public bool Authorised(HttpContext context)
        {
            return _authorizedForManagement == null ? true : _authorizedForManagement(context);
        }

        public IRequestHandler GetRequestHandler(string name)
        {
            return _container == null ? null : _container.Resolve<IRequestHandler>(name);
        }

        internal static void Initialise(Func<HttpRequest, bool> shouldProfileRequest, 
            IContainer container, 
            IEnumerable<ProfilerError> profilerErrors,
            Func<HttpContext, bool> authorizedForManagement)
        {
            _current = new RequestProfilerContext
            {
                _container = container,
                _shouldProfile = shouldProfileRequest,
                _persistentProfilerErrors = profilerErrors.ToList(),
                _authorizedForManagement = authorizedForManagement
            };
        }

        internal RequestProfilerContext()
        {}
    }
}