using System.Collections.Generic;
using System.Web;
using Castle.Core;
using Castle.MicroKernel.Proxy;
using ProductionProfiler.Configuration;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.IoC
{
    public class ProfilingInterceptorSelector : IModelInterceptorsSelector
    {
        public bool HasInterceptors(ComponentModel model)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(Constants.RequestProfileContextKey))
            {
                return RequestProfilerContext.Current.ShouldIntercept(model.Service);
            }

            return false;
        }

        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(Constants.RequestProfileContextKey))
            {
                if (!RequestProfilerContext.Current.ShouldIntercept(model.Service))
                    return interceptors;

                if (interceptors.Length == 0)
                    return new[] { new InterceptorReference(typeof(RequestProfilingInterceptor)) };

                return new List<InterceptorReference>(interceptors)
                {
                    new InterceptorReference(typeof (RequestProfilingInterceptor))
                }.ToArray();
            }

            return interceptors;
        }
    }
}
