using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.Core;
using Castle.MicroKernel.Proxy;
using ProductionProfiler.Interfaces.Resources;

namespace ProductionProfiler.IoC
{
    public class ProfilingInterceptorSelector : IModelInterceptorsSelector
    {
        private readonly IList<Type> _typesToIntercept;

        public ProfilingInterceptorSelector(IList<Type> typesToIntercept)
        {
            _typesToIntercept = typesToIntercept;
        }

        public bool HasInterceptors(ComponentModel model)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(Constants.RequestProfileContextKey))
            {
                return _typesToIntercept.Any(t => t.IsAssignableFrom(model.Service));
            }

            return false;
        }

        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(Constants.RequestProfileContextKey))
            {
                if (!_typesToIntercept.Any(t => t.IsAssignableFrom(model.Service)))
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
