using System;
using System.Collections.Generic;

namespace ProductionProfiler.Core.IoC
{
    public interface IContainer
    {
        void RegisterTransient<T>(Type implementation, string key = null) where T : class;
        void RegisterSingleton<T>(Type implementation, string key = null) where T : class;
        void RegisterPerWebRequest<T>(Type implementation, string key = null) where T : class;
        void RegisterSingletonInstance<T>(T instance) where T : class;
        void InitialiseForProxyInterception(IEnumerable<Type> typesToIntercept, IEnumerable<Type> typesToIgnore);
        T Resolve<T>();
        T Resolve<T>(string key);
        T[] ResolveAll<T>();
        bool HasObject(Type type);
    }
}
