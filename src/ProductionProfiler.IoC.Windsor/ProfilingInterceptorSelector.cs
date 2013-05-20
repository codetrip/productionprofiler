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
            if (ProfilerContext.Profiling)
            {
                return ShouldIntercept(model.Services);
            }

            return false;
        }

        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            if (ProfilerContext.Profiling)
            {
                if (!ShouldIntercept(model.Services))
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

        private bool ShouldIntercept(IEnumerable<Type> serviceTypes)
        {
	        return _typesToIntercept == null ? 
				serviceTypes.Any(serviceType => !_typesToIgnore.Any(t => t.IsAssignableFrom(serviceType))) : 
				serviceTypes.Any(serviceType => _typesToIntercept.Any(t => t.IsAssignableFrom(serviceType)) && !_typesToIgnore.Any(t => t.IsAssignableFrom(serviceType)));
        }
    }
}
