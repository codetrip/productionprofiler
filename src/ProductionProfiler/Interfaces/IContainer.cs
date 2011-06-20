
using System;
using System.Collections.Generic;

namespace ProductionProfiler.Core.Interfaces
{
    public interface IContainer
    {
        void RegisterTransient<T>(Type implementation, string key = null);
        void RegisterSingleton<T>(Type implementation, string key = null);
        void RegisterPerWebRequest<T>(Type implementation, string key = null);
        void RegisterSingletonInstance<T>(T instance);
        T Resolve<T>();
        T Resolve<T>(string key);
        void InitialiseForProxyInterception(IList<Type> typesToIntercept);
    }
}
