using System;
using System.Collections.Generic;

namespace ProductionProfiler.Core.IoC
{
    public interface IContainer
    {
        void RegisterTransient<T>(Type implementation, string key = null);
        void RegisterSingleton<T>(Type implementation, string key = null);
        void RegisterPerWebRequest<T>(Type implementation, string key = null);
        void RegisterSingletonInstance<T>(T instance);
        void InitialiseForProxyInterception(IEnumerable<Type> typesToIntercept, IEnumerable<Type> typesToIgnore);
        T Resolve<T>();
        T Resolve<T>(string key);
        T[] ResolveAll<T>();
        bool HasObject(Type type);
    }
}
