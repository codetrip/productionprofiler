using System;
using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Factory;
using ProductionProfiler.Core.Handlers;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Profiling
{
    public class ProfilerContext
    {
        private const string ProfileContextKey = "profile-context-key";
        private static IContainer _container;
        private static ProfilerConfiguration _configuration;
        private IRequestProfiler _profiler;

        #region Internal Constructors

        internal static void Initialise(IContainer container, ProfilerConfiguration configuration)
        {
            _configuration = configuration;
            _container = container;
        }

        private ProfilerContext()
        {}

        #endregion

        public static ProfilerContext Current
        {
            get 
            {
                if (HttpContextFactory.GetHttpContext().Items[ProfileContextKey] == null)
                {
                    HttpContextFactory.GetHttpContext().Items[ProfileContextKey] = new ProfilerContext();
                }

                return HttpContextFactory.GetHttpContext().Items[ProfileContextKey] as ProfilerContext;
            }
        }

        public IRequestProfiler Profiler
        {
            get { return _profiler ?? (_profiler = _container.Resolve<IRequestProfiler>()); }
        }

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
            return _configuration.ShouldProfile != null && (!request.RawUrl.Contains("/profiler") && _configuration.ShouldProfile(request));
        }

        public bool MonitoringEnabled
        {
            get { return _configuration.MonitoringEnabled; }
        }

        public void StartProfiling(HttpContext context)
        {
            Profiler.StartProfiling(context);
            HttpContextFactory.GetHttpContext().Items[Constants.RequestProfileContextKey] = true;
        }

        public void StopProfiling(HttpResponse response)
        {
            if (ProfilingCurrentRequest())
            {
                HttpContextFactory.GetHttpContext().Items.Remove(Constants.RequestProfileContextKey);
                Profiler.StopProfiling(response);
            }
        }

        public void Exception(Exception e)
        {
            System.Diagnostics.Trace.Write(e.Format());

            if (_configuration.ReportException != null)
            {
                _configuration.ReportException(e);
            }
        }

        public bool ProfilingCurrentRequest()
        {
            return HttpContextFactory.GetHttpContext().Items.Contains(Constants.RequestProfileContextKey);
        }

        public bool AuthorisedForManagement(HttpContext context)
        {
            return _configuration.AuthorizedForManagement == null || _configuration.AuthorizedForManagement(context);
        }

        public IRequestHandler GetRequestHandler(string name)
        {
            return _container == null ? null : _container.Resolve<IRequestHandler>(name);
        }
    }
}