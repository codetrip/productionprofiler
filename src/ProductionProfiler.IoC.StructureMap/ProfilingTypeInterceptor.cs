using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Profiling;
using StructureMap;
using StructureMap.Interceptors;

namespace ProductionProfiler.IoC.StructureMap
{
    public class ProfilingTypeInterceptor : TypeInterceptor
    {
        private readonly IEnumerable<Type> _typesToIntercept;
        private readonly IEnumerable<Type> _typesToIgnore;
        private readonly ProxyGenerator _proxyGenerator;

        public ProfilingTypeInterceptor(ProxyGenerator proxyGenerator, IEnumerable<Type> typesToIntercept, IEnumerable<Type> typesToIgnore)
        {
            _typesToIntercept = typesToIntercept;
            _typesToIgnore = typesToIgnore;
            _proxyGenerator = proxyGenerator;
        }

        public object Process(object target, IContext context)
        {
            if (ProfilerContext.Profiling)
            {
                var interfaces = target.GetType().GetInterfaces();

                if(interfaces.Any())
                {
                    var decorator = _proxyGenerator.CreateInterfaceProxyWithTarget(interfaces.First(), interfaces.Skip(1).ToArray(), target, new RequestProfilingInterceptor());
                    return decorator;
                }
            }

            return target;
        }

        public bool MatchesType(Type serviceType)
        {
            if (!ProfilerContext.Profiling)
                return false;

            return (_typesToIntercept == null || _typesToIntercept.Any(t => t.IsAssignableFrom(serviceType))) && !_typesToIgnore.Any(t => t.IsAssignableFrom(serviceType));
        }
    }
}
