using System;
using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Handlers;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Profiling
{
    public class ProfilerContext
    {
        private IContainer _container;
        private Func<HttpRequest, bool> _shouldProfile;
        private static ProfilerContext _current = new ProfilerContext();
        private Func<HttpContext, bool> _authorizedForManagement;
        private Action<Exception> _reportException;
        private ProfilerConfiguration _configuration;

        #region Internal Constructors

        internal static void Initialise(Func<HttpRequest, bool> shouldProfileRequest,
            IContainer container,
            Func<HttpContext, bool> authorizedForManagement,
            Action<Exception> reportException,
            ProfilerConfiguration configuration)
        {
            _current = new ProfilerContext
            {
                _container = container,
                _shouldProfile = shouldProfileRequest,
                _authorizedForManagement = authorizedForManagement,
                _reportException = reportException,
                _configuration = configuration
            };
        }

        internal ProfilerContext()
        { }

        #endregion

        public static ProfilerContext Current
        {
            get
            {
                return _current;
            }
        }

        public IRequestProfiler Profiler { get; private set; }

        public IEnumerable<IMethodDataCollector> GetMethodDataCollectorsForType(Type targetType)
        {
            return _configuration.MethodDataCollectorMappings.GetMethodDataCollectorsForType(targetType, _container);
        }

        public void BeginRequest(HttpContext context)
        {
            _container.Resolve<IRequestProfilingCoordinator>().BeginRequest(context);
        }

        public void EndRequest(HttpContext context)
        {
            _container.Resolve<IRequestProfilingCoordinator>().EndRequest(context);
        }

        public bool ShouldProfile(HttpRequest request)
        {
            return _shouldProfile == null ? false : !request.RawUrl.Contains("/profiler") && _shouldProfile(request);
        }

        public void StartProfiling(HttpContext context)
        {
            Profiler = _container.Resolve<IRequestProfiler>();
            Profiler.StartProfiling(context);
            HttpContext.Current.Items[Constants.RequestProfileContextKey] = true;
        }

        public void StopProfiling(HttpResponse response)
        {
            if (ProfilingCurrentRequest())
            {
                HttpContext.Current.Items.Remove(Constants.RequestProfileContextKey);

                if (Profiler != null)
                {
                    Profiler.StopProfiling(response);
                    Profiler = null;
                }
            }
        }

        public void Exception(Exception e)
        {
            System.Diagnostics.Trace.Write(e.Format());

            if(_reportException != null)
            {
                _reportException(e);
            }
        }

        public bool ProfilingCurrentRequest()
        {
            return HttpContext.Current.Items.Contains(Constants.RequestProfileContextKey) && Profiler != null;
        }

        public bool AuthorisedForManagement(HttpContext context)
        {
            return _authorizedForManagement == null ? true : _authorizedForManagement(context);
        }

        public IRequestHandler GetRequestHandler(string name)
        {
            return _container == null ? null : _container.Resolve<IRequestHandler>(name);
        }
    }
}