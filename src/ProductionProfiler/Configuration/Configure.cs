
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using ProductionProfiler.Core.Binding;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Handlers;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Log4Net;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Configuration
{
    public class Configure : IFluentConfiguration
    {
        private bool _requestDataCollectorSet;
        private bool _responseDataCollectorSet;
        private bool _cacheEngineSet;
        private List<Type> _typesToIntercept;
        private Func<HttpRequest, bool> _requestFilter;
        private IContainer _container;
        private readonly List<ProfilerError> _profilerErrors = new List<ProfilerError>();
        private ProfilerConfiguration _profilerConfiguration;

        public static IFluentConfiguration With(IContainer container)
        {
            Configure config = new Configure
            {
                _profilerConfiguration = new ProfilerConfiguration(),
                _requestFilter = req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty,
                _container = container
            };

            return config;
        }

        IFluentConfiguration IFluentConfiguration.Intercept(Type[] typesToIntercept)
        {
            _typesToIntercept = new List<Type>(new[] {typeof (IWantToBeProfiled)});
            _typesToIntercept.AddRange(typesToIntercept);
            return this;
        }

        IFluentConfiguration IFluentConfiguration.RequestFilter(Func<HttpRequest, bool> requestFilter)
        {
            _requestFilter = requestFilter;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.CaptureExceptions()
        {
            _profilerConfiguration.CaptureExceptions = true;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.CaptureResponse<T>(Func<HttpContext, T> responseFilterConstructor)
        {
            _profilerConfiguration.CaptureResponse = true;
            _profilerConfiguration.GetResponseFilter = responseFilterConstructor;
            return this;
        }

        public IFluentConfiguration CacheEngine<T>() where T : ICacheEngine
        {
            if (_cacheEngineSet)
            {
                _profilerErrors.Add(new ProfilerError
                {
                    Message = string.Format("ICacheEngine has already been specified, cache engine of type {0} was not registered".FormatWith(typeof(T).FullName)),
                    Type = ProfilerErrorType.Configuration,
                });
                return this;
            }

            _cacheEngineSet = true;
            _container.RegisterTransient<ICacheEngine>(typeof(T));
            return this;
        }

        public IFluentCollectorConfiguration AddMethodEntryDataCollector<T>() where T : IMethodEntryDataCollector
        {
            return new CollectorConfiguration<IMethodEntryDataCollector>(this, typeof(T));
        }

        public IFluentCollectorConfiguration AddMethodExitDataCollector<T>() where T : IMethodExitDataCollector
        {
            return new CollectorConfiguration<IMethodExitDataCollector>(this, typeof(T));
        }

        public IFluentConfiguration HttpRequestDataCollector<T>() where T : IHttpRequestDataCollector
        {
            if (_requestDataCollectorSet)
            {
                _profilerErrors.Add(new ProfilerError
                {
                    Message = string.Format("IHttpRequestDataCollector has already been specified, http request data collector of type {0} was not registered".FormatWith(typeof(T).FullName)),
                    Type = ProfilerErrorType.Configuration,
                });
            }

            _requestDataCollectorSet = true;
            _container.RegisterTransient<IHttpRequestDataCollector>(typeof(T));
            return this;
        }

        public IFluentConfiguration HttpResponseDataCollector<T>() where T : IHttpResponseDataCollector
        {
            if (_responseDataCollectorSet)
            {
                _profilerErrors.Add(new ProfilerError
                {
                    Message = string.Format("IHttpResponseDataCollector has already been specified, http response data collector of type {0} was not registered".FormatWith(typeof(T).FullName)),
                    Type = ProfilerErrorType.Configuration,
                });
            }

            _responseDataCollectorSet = true;
            _container.RegisterTransient<IHttpResponseDataCollector>(typeof(T));
            return this;
        }

        IFluentConfiguration IFluentConfiguration.Log4Net(string loggerName)
        {
            var profilingLogger = LogManager.Exists(loggerName);

            if (profilingLogger == null)
            {
                _profilerErrors.Add(new ProfilerError
                {
                    Message = string.Format("No log4net logger named {0} was found in the log4net configuration", loggerName),
                    Type = ProfilerErrorType.Configuration,
                });
                return this;
            }

            var logger = profilingLogger.Logger as Logger;

            if (logger != null)
            {
                logger.Level = Level.Debug;

                var profilingAppender = new Log4NetProfilingAppender
                {
                    Threshold = Level.Debug,
                    Name = Constants.ProfilingAppender
                };

                logger.AddAppender(profilingAppender);

                Hierarchy repository = LogManager.GetRepository() as Hierarchy;

                if (repository != null)
                {
                    repository.Configured = true;
                    repository.RaiseConfigurationChanged(EventArgs.Empty);  
                }

                _profilerConfiguration.ProfilingAppender = profilingAppender;
            }

            _profilerConfiguration.Log4NetEnabled = true;

            return this;
        }

        public IFluentConfiguration EnableMonitoring(long postThreshold, long getThreshold)
        {
            _profilerConfiguration.GetRequestThreshold = getThreshold;
            _profilerConfiguration.PostRequestThreshold = postThreshold;
            _profilerConfiguration.MonitoringEnabled = true;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.DataProvider(IPersistenceProvider persistenceProvider)
        {
            _container.RegisterTransient<IProfilerRepository>(persistenceProvider.RepositoryType);
            persistenceProvider.RegisterDependentComponents(_container);
            return this;
        }

        void IFluentConfiguration.Initialise()
        {
            RegisterDependencies();
            RequestProfilerContext.Initialise(_requestFilter, _container, _profilerConfiguration.MonitoringEnabled, _profilerErrors);
        }

        private void RegisterDependencies()
        {
            _container.RegisterPerWebRequest<IRequestProfiler>(typeof(RequestProfiler));
            _container.RegisterSingletonInstance(_profilerConfiguration);
            _container.RegisterTransient<IRequestHandler>(typeof(UpdateProfiledRequestHandler), Constants.Handlers.UpdateProfiledRequest);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewResultsRequestHandler), Constants.Handlers.Results);
            _container.RegisterTransient<IRequestHandler>(typeof(AddProfiledRequestHandler), Constants.Handlers.AddProfiledRequest);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewProfiledRequestsHandler), Constants.Handlers.ViewProfiledRequests);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewResponseRequestHandler), Constants.Handlers.ViewResponse);
            _container.RegisterTransient<IRequestHandler>(typeof(DeleteProfiledDataByIdRequestHandler), Constants.Handlers.DeleteProfiledRequestDataById);
            _container.RegisterTransient<IRequestHandler>(typeof(DeleteProfiledDataByUrlRequestHandler), Constants.Handlers.DeleteProfiledRequestDataByUrl);
            _container.RegisterTransient<IAddProfiledRequestRequestBinder>(typeof(AddProfiledRequestRequestBinder));
            _container.RegisterTransient<IUpdateProfiledRequestRequestBinder>(typeof(UpdateProfiledRequestRequestBinder));
            _container.RegisterTransient<IRequestProfilingCoordinator>(typeof(RequestProfilingCoordinator));

            if (!_requestDataCollectorSet)
                _container.RegisterTransient<IHttpRequestDataCollector>(typeof(NullHttpRequestDataCollector));
            if (!_responseDataCollectorSet)
                _container.RegisterTransient<IHttpResponseDataCollector>(typeof(NullHttpResponseDataCollector));
            if (!_cacheEngineSet)
                _container.RegisterTransient<ICacheEngine>(typeof(HttpRuntimeCacheEngine));

            _container.InitialiseForProxyInterception(_typesToIntercept);
        }

        private class CollectorConfiguration<T> : IFluentCollectorConfiguration
        {
            private readonly Configure _configureInstance;
            private readonly Type _collectorType;

            public CollectorConfiguration(Configure configureInstance, Type collectorType)
            {
                _configureInstance = configureInstance;
                _collectorType = collectorType;
            }

            public IFluentConfiguration ForTypes(IEnumerable<Type> types)
            {
                if (_configureInstance._profilerConfiguration.CollectorTypeMappings.ContainsKey(_collectorType))
                {
                    _configureInstance._profilerErrors.Add(new ProfilerError
                    {
                        Message = string.Format("{0} has already been registered for method entry data collector of type {1}.".FormatWith(typeof(T).FullName, _collectorType.FullName)),
                        Type = ProfilerErrorType.Configuration,
                    });
                }
                else
                {
                    _configureInstance._container.RegisterTransient<T>(_collectorType, _collectorType.FullName);
                    _configureInstance._profilerConfiguration.CollectorTypeMappings.Add(_collectorType, types);
                }

                return _configureInstance;
            }
        }
    }

    public interface IFluentConfiguration
    {
        IFluentConfiguration Intercept(Type[] typesToIntercept = null);
        IFluentConfiguration RequestFilter(Func<HttpRequest, bool> requestFilter);
        IFluentConfiguration EnableMonitoring(long postThreshold, long getThreshold);
        IFluentConfiguration CaptureExceptions();
        IFluentConfiguration CaptureResponse<T>(Func<HttpContext, T> responseFilterConstructor) where T : Stream, IResponseFilter;
        IFluentCollectorConfiguration AddMethodEntryDataCollector<T>() where T : IMethodEntryDataCollector;
        IFluentCollectorConfiguration AddMethodExitDataCollector<T>() where T : IMethodExitDataCollector;
        IFluentConfiguration Log4Net(string loggerName);
        IFluentConfiguration DataProvider(IPersistenceProvider persistenceProvider);
        IFluentConfiguration CacheEngine<T>() where T : ICacheEngine;
        IFluentConfiguration HttpRequestDataCollector<T>() where T : IHttpRequestDataCollector;
        IFluentConfiguration HttpResponseDataCollector<T>() where T : IHttpResponseDataCollector;
        void Initialise();
    }

    public interface IFluentCollectorConfiguration
    {
        IFluentConfiguration ForTypes(IEnumerable<Type> types);
    }
}
