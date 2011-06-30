using System;
using System.Web;
using ProductionProfiler.Core.Handlers;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Profiling
{
    public class RequestProfilerContext
    {
        private IContainer _container;
        private Func<HttpRequest, bool> _shouldProfile;
        private static RequestProfilerContext _current = new RequestProfilerContext();
        private Func<HttpContext, bool> _authorizedForManagement;
        private Action<Exception> _reportException;

        public static RequestProfilerContext Current
        {
            get
            {
                return _current;
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

        public void Exception(Exception e)
        {
            if(_reportException != null)
            {
                System.Diagnostics.Trace.Write(e.Format());
                _reportException(e);
            }
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
            Func<HttpContext, bool> authorizedForManagement,
            Action<Exception> reportException)
        {
            _current = new RequestProfilerContext
            {
                _container = container,
                _shouldProfile = shouldProfileRequest,
                _authorizedForManagement = authorizedForManagement,
                _reportException = reportException
            };
        }

        internal RequestProfilerContext()
        {}
    }
}