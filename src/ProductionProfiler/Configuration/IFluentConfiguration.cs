using System;
using System.Collections.Generic;
using System.Web;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Logging;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Serialization;

namespace ProductionProfiler.Core.Configuration
{
    public interface IFluentConfiguration
    {
        /// <summary>
        /// Set of types that should be intercepted and profiled, if this method is not called during configuration
        /// All types resolved from the configured container will be intercepted.
        /// 
        /// We determine whether to intercept a type by checking if it is assignable from any of the types in the typesToIntercept argument
        /// i.e. _typesToIntercept.Any(t => t.IsAssignableFrom(interceptedType))
        /// </summary>
        /// <param name="typesToIntercept">List of types to intercept</param>
        /// <returns></returns>
        IFluentConfiguration TypesToIntercept(IEnumerable<Type> typesToIntercept);
        /// <summary>
        /// Set of types which should not be intercepted
        /// </summary>
        /// <param name="typesToIgnore">List of types to intercept</param>
        /// <returns></returns>
        IFluentConfiguration TypesToIgnore(IEnumerable<Type> typesToIgnore);
        /// <summary>
        /// Delegate used to filter requests that should trigger the profiling module
        /// </summary>
        /// <param name="requestFilter"></param>
        /// <returns></returns>
        IFluentConfiguration RequestFilter(Func<HttpRequest, bool> requestFilter);
        IFluentConfiguration Logger(ILogger logger);
        IFluentConfiguration DataProvider(IPersistenceProvider persistenceProvider);
        IFluentConfiguration CacheEngine<T>() where T : IProfilerCacheEngine;
        IFluentConfiguration Serializer<T>() where T : ISerializer;
        IFluentConfiguration HttpRequestDataCollector<T>() where T : IHttpRequestDataCollector;
        IFluentConfiguration HttpResponseDataCollector<T>() where T : IHttpResponseDataCollector;
        IFluentConfiguration AuthoriseManagement(Func<HttpContext, bool> authorisedForManagement);
        IFluentConfiguration AuthoriseSession(Func<string, bool> authoriseSession);
        IFluentConfiguration CollectMethodDataForTypes(IEnumerable<Type> typesToCollectMethodDataFor);
        IFluentConfiguration TimeAllRequests(int durationThresholdMs);
        IFluentConfiguration DoNotTimeAllRequests();
        IFluentProfilingTriggerConfiguration Trigger { get; }
        IFluentCollectorConfiguration AddMethodInvocationDataCollector<T>() where T : IMethodInvocationDataCollector;
        void Initialise();
    }
}