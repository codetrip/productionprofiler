using System;
using System.Web;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Modules
{
    public class RequestProfilingModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
            context.EndRequest += context_EndRequest;
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                var httpContext = ((HttpApplication)sender).Context;

                if (RequestProfilerContext.Current.ShouldProfile(httpContext.Request))
                {
                    RequestProfilerContext.Current.StartProfiling();
                    var profilingManager = RequestProfilerContext.Current.Container.Resolve<IRequestProfilingCoordinator>();
                    profilingManager.BeginRequest(httpContext);
                }
            }
            catch(Exception ex)
            {
                RequestProfilerContext.Current.StopProfiling();
                System.Diagnostics.Debug.Write(ex.Format());
            }
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            try
            {
                var httpContext = ((HttpApplication)sender).Context;

                if (RequestProfilerContext.Current.ShouldProfile(httpContext.Request) && RequestProfilerContext.Current.ProfilingCurrentRequest())
                {
                    RequestProfilerContext.Current.StopProfiling();

                    var profilingManager = RequestProfilerContext.Current.Container.Resolve<IRequestProfilingCoordinator>();
                    profilingManager.EndRequest(httpContext);
                }
            }
            catch (Exception ex)
            {
                RequestProfilerContext.Current.StopProfiling();
                System.Diagnostics.Debug.Write(ex.Format());
            }
            
        }

        public void Dispose()
        { }
    }
}
