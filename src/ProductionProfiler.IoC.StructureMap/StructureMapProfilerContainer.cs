using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using StructureMap;
using System.Linq;
using StructureMap.Pipeline;
using PP = ProductionProfiler.Core;

namespace ProductionProfiler.IoC.StructureMap
{
    public class StructureMapProfilerContainer : PP.IoC.IContainer
    {
        private readonly IContainer _container;

        public StructureMapProfilerContainer(IContainer container)
        {
            _container = container;
        }

        public void RegisterTransient<T>(Type implementation, string name = null)
        {
            _container.Configure(c => c.For(typeof(T))
                .Use(implementation)
                .ConditionalName(name));
        }

        public void RegisterSingleton<T>(Type implementation, string name = null)
        {
            _container.Configure(c => c.For(typeof(T))
                .Singleton()
                .Use(implementation)
                .ConditionalName(name));
        }

        public void RegisterPerWebRequest<T>(Type implementation, string name = null)
        {
            _container.Configure(c => c.For(typeof(T))
                .HybridHttpOrThreadLocalScoped()
                .Use(implementation)
                .ConditionalName(name));
        }

        public void RegisterSingletonInstance<T>(T instance)
        {
            _container.Configure(c => c.ForSingletonOf<T>().Add(instance));
        }

        public T Resolve<T>()
        {
            return _container.GetInstance<T>();
        }

        public T Resolve<T>(string key)
        {
            return _container.GetInstance<T>(key);
        }

        public T[] ResolveAll<T>()
        {
            return _container.GetAllInstances<T>().ToArray();
        }

        public void InitialiseForProxyInterception(IEnumerable<Type> typesToIntercept, IEnumerable<Type> typesToIgnore)
        {
            _container.Configure(c => c.RegisterInterceptor(new ProfilingTypeInterceptor(new ProxyGenerator(), typesToIntercept, typesToIgnore)));
        }
    }

    public static class ContainerExtensions
    {
        public static ConfiguredInstance ConditionalName(this ConfiguredInstance instance, string name)
        {
            if (name != null)
                instance.Named(name);

            return instance;
        }
    }
}
