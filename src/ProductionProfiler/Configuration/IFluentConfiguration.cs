using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Logging;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling;
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
        /// <summary>
        /// If set during configuration the profiler will monitor all requests and log any requests
        /// that exceed the thresholds set when configuring monitoring
        /// </summary>
        /// <param name="postThreshold">Maximum number of milliseconds a POST request should take before being flagged</param>
        /// <param name="getThreshold">Maximum number of milliseconds a GET request should take before being flagged</param>
        /// <returns></returns>
        IFluentConfiguration EnableMonitoring(long postThreshold, long getThreshold);
        /// <summary>
        /// If set, any exception that occurs in an intercepted method will be logged with the profile data.
        /// </summary>
        /// <returns></returns>
        IFluentConfiguration CaptureExceptions();
        /// <summary>
        /// Allows you to capture the response using your own custom ResponseFilter, your filter must implement the 
        /// IResponseFilter interface which allows the profiler to capture the response body and store it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="responseFilterConstructor">delegate to construct a response filter to apply to profiled responses</param>
        /// <returns></returns>
        IFluentConfiguration CaptureResponse<T>(Func<HttpContext, T> responseFilterConstructor) where T : Stream, IResponseFilter;
        /// <summary>
        /// If set profiler will use the StoreResponseFilter which reads the HttpResponse.OutputStream and stores
        /// it via the persistence provider linked to the profiled request data.
        /// </summary>
        /// <returns></returns>
        IFluentConfiguration CaptureResponse();
        IFluentConfiguration Logger(ILogger logger);
        IFluentConfiguration DataProvider(IPersistenceProvider persistenceProvider);
        IFluentConfiguration CacheEngine<T>() where T : IProfilerCacheEngine;
        IFluentConfiguration Serializer<T>() where T : ISerializer;
        IFluentConfiguration HttpRequestDataCollector<T>() where T : IHttpRequestDataCollector;
        IFluentConfiguration HttpResponseDataCollector<T>() where T : IHttpResponseDataCollector;
        IFluentConfiguration Authorize(Func<HttpContext, bool> authorisedForManagement);
        IFluentConfiguration CollectInputOutputMethodDataForTypes(IEnumerable<Type> typesToCollectInputOutputDataFor);
        IFluentCollectorConfiguration AddMethodDataCollector<T>() where T : IMethodDataCollector;
        void Initialise();
    }
}