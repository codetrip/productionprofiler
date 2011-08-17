using System;
using System.Linq;
using System.Web;
using ProductionProfiler.Core.Auditing;
using ProductionProfiler.Core.Profiling;

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

            var context = ((HttpApplication)sender).Context;

            try
            {
                if (ProfilerContext.Configuration.ShouldProfileRequest(context.Request))
                {
                    var coordinatorsForCurrentRequest = ProfilerContext.Configuration.GetCoordinators(context);

                    if (coordinatorsForCurrentRequest.Any())
                    {
                        ProfilerContext.Profiler.Start(context, coordinatorsForCurrentRequest);
                    }
                }
            }
            catch(Exception ex)
            {
                Error(ex);

                try
                {
                    ProfilerContext.Profiler.Stop(context.Response);
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
                ProfilerContext.Profiler.Stop(((HttpApplication)sender).Context.Response);
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
