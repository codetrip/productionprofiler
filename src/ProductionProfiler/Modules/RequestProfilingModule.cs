﻿using System;
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
            try
            {
                var httpContext = ((HttpApplication)sender).Context;

                if (RequestProfilerContext.Current.ShouldProfile(httpContext.Request))
                {
                    RequestProfilerContext.Current.Container.Resolve<IRequestProfilingCoordinator>().BeginRequest(httpContext);
                }
            }
            catch(Exception ex)
            {
                RequestProfilerContext.Current.Exception(ex);
                RequestProfilerContext.Current.StopProfiling();
            }
        }

        void EndRequest(object sender, EventArgs e)
        {
            try
            {
                if (RequestProfilerContext.Current.ProfilingCurrentRequest())
                {
                    RequestProfilerContext.Current.Container.Resolve<IRequestProfilingCoordinator>().EndRequest(((HttpApplication)sender).Context);
                }
            }
            catch (Exception ex)
            {
                RequestProfilerContext.Current.Exception(ex);
                RequestProfilerContext.Current.StopProfiling();
            }
        }

        public void Dispose()
        { }
    }
}
