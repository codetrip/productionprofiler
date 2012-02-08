using System;
using System.Linq;
using System.Web;
using ProductionProfiler.Core.Auditing;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.Core.Modules
{
    public class RequestProfilingModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequest;
            context.EndRequest += EndRequest;
        }

        void BeginRequest(object sender, EventArgs e)
        {
            if (!ProfilerContext.Initialised)
                return;

            var httpContext = ((HttpApplication)sender).Context;

            try
            {
                if (ProfilerContext.Configuration.ShouldProfileRequest(httpContext.Request))
                {
                    var coordinatorsForCurrentRequest = ProfilerContext.Configuration.GetCoordinators(httpContext).ToList();

                    var requestProfileContext = new RequestProfileContext(httpContext, coordinatorsForCurrentRequest);
                    httpContext.Items[Constants.RequestProfileContextHttpContextItemKey] = requestProfileContext;

                    if (coordinatorsForCurrentRequest.Any())
                    {
                        ProfilerContext.Profiler.Start(new RequestProfileContext(httpContext, coordinatorsForCurrentRequest));
                    }
                }

                if (ProfilerContext.Configuration.ShouldTimeRequest(httpContext.Request))
                {
                    ProfilerContext.RequestTimer.Start(httpContext);
                }
            }
            catch(Exception ex)
            {
                Error(ex);

                try
                {
                    ProfilerContext.Profiler.Stop();
                }
                catch (Exception innerEx)
                {
                    Error(innerEx);
                }
            }
        }

        void EndRequest(object sender, EventArgs e)
        {
            if (!ProfilerContext.Initialised)
                return;

            try
            {
                var httpContext = ((HttpApplication) sender).Context;
                
                ProfilerContext.Profiler.Stop();
                if (ProfilerContext.Configuration.TimeAllRequests)
                    ProfilerContext.RequestTimer.Stop(httpContext);
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        private void Error(Exception e)
        {
            try
            {
                var auditor = ProfilerContext.Container.Resolve<IComponentAuditor>();
                if (auditor != null)
                {
                    auditor.Error(GetType(), e);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Write(ExceptionUtility.FormatException(e));
                System.Diagnostics.Trace.Write(ExceptionUtility.FormatException(ex));
            }
        }

        public void Dispose()
        { }
    }
}
