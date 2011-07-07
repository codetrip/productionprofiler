using System;
using System.Web;
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
            var httpContext = ((HttpApplication)sender).Context;

            try
            {
                if (ProfilerContext.Current.ShouldProfile(httpContext.Request))
                {
                    ProfilerContext.Current.BeginRequest(httpContext);
                }
            }
            catch(Exception ex)
            {
                ProfilerContext.Current.Exception(ex);

                try
                {
                    ProfilerContext.Current.StopProfiling(((HttpApplication)sender).Context.Response);
                }
                catch (Exception innerEx)
                {
                    ProfilerContext.Current.Exception(innerEx);
                }
            }
        }

        void EndRequest(object sender, EventArgs e)
        {
            try
            {
                if (ProfilerContext.Current.ProfilingCurrentRequest() || ProfilerContext.Current.MonitoringEnabled)
                {
                    ProfilerContext.Current.EndRequest(((HttpApplication)sender).Context);
                }
            }
            catch (Exception ex)
            {
                ProfilerContext.Current.Exception(ex);

                try
                {
                    ProfilerContext.Current.StopProfiling(((HttpApplication)sender).Context.Response);
                }
                catch (Exception innerEx)
                {
                    ProfilerContext.Current.Exception(innerEx);
                }
            }
        }

        public void Dispose()
        { }
    }
}
