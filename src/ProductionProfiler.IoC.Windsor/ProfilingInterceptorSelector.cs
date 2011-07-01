using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel.Proxy;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.IoC.Windsor
{
    public class ProfilingInterceptorSelector : IModelInterceptorsSelector
    {
        private readonly IEnumerable<Type> _typesToIntercept;
        private readonly IEnumerable<Type> _typesToIgnore;

        public ProfilingInterceptorSelector(IEnumerable<Type> typesToIntercept, IEnumerable<Type> typesToIgnore)
        {
            _typesToIntercept = typesToIntercept;
            _typesToIgnore = typesToIgnore;
        }

        public bool HasInterceptors(ComponentModel model)
        {
            if (ProfilerContext.Current.ProfilingCurrentRequest())
            {
                return ShouldIntercept(model.Service);
            }

            return false;
        }

        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            if (ProfilerContext.Current.ProfilingCurrentRequest())
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
            return (_typesToIntercept == null || _typesToIntercept.Any(t => t.IsAssignableFrom(serviceType))) && !_typesToIgnore.Any(t => t.IsAssignableFrom(serviceType));
        }
    }
}
