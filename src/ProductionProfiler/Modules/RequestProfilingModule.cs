using System;
using System.Web;
using ProductionProfiler.Core.Interfaces.Entities;

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
            var httpContext = ((HttpApplication)sender).Context;

            if (RequestProfilerContext.Current.ShouldProfile(httpContext.Request))
            {
                var profilingManager = RequestProfilerContext.Current.GetRequestProfilingManager();
                profilingManager.BeginRequest(httpContext);
            }
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            var httpContext = ((HttpApplication) sender).Context;

            if (RequestProfilerContext.Current.ShouldProfile(httpContext.Request))
            {
                var profilingManager = RequestProfilerContext.Current.GetRequestProfilingManager();
                profilingManager.EndRequest(httpContext);
            }
        }

        public void Dispose()
        { }
    }
}
