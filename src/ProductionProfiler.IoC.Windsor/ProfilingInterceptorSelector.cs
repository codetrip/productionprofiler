using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel.Proxy;
using ProductionProfiler.Core.Resources;

namespace ProductionProfiler.IoC.Windsor
{
    public class ProfilingInterceptorSelector : IModelInterceptorsSelector
    {
        private static readonly Type _interceptorType = typeof (IInterceptor);
        private readonly IList<Type> _typesToIntercept;

        public ProfilingInterceptorSelector(IList<Type> typesToIntercept)
        {
            _typesToIntercept = typesToIntercept;
        }

        public bool HasInterceptors(ComponentModel model)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(Constants.RequestProfileContextKey))
            {
                return ShouldIntercept(model.Service);
            }

            return false;
        }

        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(Constants.RequestProfileContextKey))
            {
                if (!ShouldIntercept(model.Service))
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

        private bool ShouldIntercept(Type serviceType)
        {
            return (_typesToIntercept == null || _typesToIntercept.Any(t => t.IsAssignableFrom(serviceType))) && !_interceptorType.IsAssignableFrom(serviceType);
        }
    }
}
