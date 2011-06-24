
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
using ProductionProfiler.Core.Serialization;

namespace ProductionProfiler.Core.Configuration
{
    public class Configure : IFluentConfiguration
    {
        private bool _requestDataCollectorSet;
        private bool _responseDataCollectorSet;
        private bool _cacheEngineSet;
        private bool _serializerSet;
        private IEnumerable<Type> _typesToIntercept;
        private IEnumerable<Type> _typesToIgnore;
        private Func<HttpRequest, bool> _requestFilter;
        private IContainer _container;
        private readonly List<ProfilerError> _profilerErrors = new List<ProfilerError>();
        private ProfilerConfiguration _profilerConfiguration;
        private Func<HttpContext, bool> _authorisedForManagement;

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

        IFluentConfiguration IFluentConfiguration.TypesToIntercept(IEnumerable<Type> typesToIntercept)
        {
            _typesToIntercept = typesToIntercept;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.TypesToIgnore(IEnumerable<Type> typesToIgnore)
        {
            _typesToIgnore = typesToIgnore;
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

        IFluentConfiguration IFluentConfiguration.CaptureResponse()
        {
            _profilerConfiguration.CaptureResponse = true;
            _profilerConfiguration.GetResponseFilter = context => new StoreResponseFilter(context.Response.Filter);
            return this;
        }

        IFluentConfiguration IFluentConfiguration.CacheEngine<T>()
        {
            if (_cacheEngineSet)
            {
                _profilerErrors.Add(new ProfilerError
                {
                    Message = string.Format("IProfilerCacheEngine has already been specified, cache engine of type {0} was not registered".FormatWith(typeof(T).FullName)),
                    Type = ProfilerErrorType.Configuration,
                });
                return this;
            }

            _cacheEngineSet = true;
            _container.RegisterTransient<IProfilerCacheEngine>(typeof(T));
            return this;
        }

        IFluentConfiguration IFluentConfiguration.Serializer<T>()
        {
            if (_serializerSet)
            {
                _profilerErrors.Add(new ProfilerError
                {
                    Message = string.Format("ISerializer has already been specified, serializer of type {0} was not registered".FormatWith(typeof(T).FullName)),
                    Type = ProfilerErrorType.Configuration,
                });
                return this;
            }

            _serializerSet = true;
            _container.RegisterTransient<ISerializer>(typeof(T));
            return this;
        }

        IFluentConfiguration IFluentConfiguration.EnableMonitoring(long postThreshold, long getThreshold)
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

        #region Collectors

        public IFluentConfiguration Authorize(Func<HttpContext, bool> authorisedForManagement)
        {
            _authorisedForManagement = authorisedForManagement;
            return this;
        }

        IFluentCollectorConfiguration IFluentConfiguration.AddMethodDataCollector<T>()
        {
            return new CollectorConfiguration(this, typeof(T));
        }

        IFluentConfiguration IFluentConfiguration.HttpRequestDataCollector<T>()
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

        IFluentConfiguration IFluentConfiguration.HttpResponseDataCollector<T>()
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

        #endregion

        #region Log4Net

        IFluentConfiguration IFluentConfiguration.Log4Net(IEnumerable<string> loggerNames)
        {
            foreach(string loggerName in loggerNames)
            {
                var profilingLogger = LogManager.Exists(loggerName);

                if (profilingLogger == null)
                {
                    _profilerErrors.Add(new ProfilerError
                    {
                        Message = string.Format("No log4net logger named {0} was found in the log4net configuration", loggerName),
                        Type = ProfilerErrorType.Configuration,
                    });

                    continue;
                }

                var logger = profilingLogger.Logger as Logger;

                if (logger != null)
                {
                    var profilingAppender = new Log4NetProfilingAppender
                    {
                        Threshold = Level.Debug,
                        Name = "{0}-{1}".FormatWith(loggerName, Constants.ProfilingAppender)
                    };

                    logger.AddAppender(profilingAppender);
                    _profilerConfiguration.ProfilingAppenders.Add(profilingAppender);
                }
            }

            Hierarchy repository = LogManager.GetRepository() as Hierarchy;

            if (repository != null)
            {
                repository.Configured = true;
                repository.RaiseConfigurationChanged(EventArgs.Empty);
            }

            _profilerConfiguration.Log4NetEnabled = true;

            return this;
        }

        #endregion

        #region Initialise & Dependencies

        void IFluentConfiguration.Initialise()
        {
            RegisterDependencies();
            RequestProfilerContext.Initialise(_requestFilter, _container, _profilerErrors, _authorisedForManagement);
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
                _container.RegisterTransient<IProfilerCacheEngine>(typeof(HttpRuntimeCacheEngine));
            if (!_serializerSet)
                _container.RegisterTransient<ISerializer>(typeof(JsonSerializer));

            _container.InitialiseForProxyInterception(_typesToIntercept,
                new List<Type>(_typesToIgnore ?? new Type[0])
                {
                    typeof (IDoNotWantToBeProfiled)
                });
        }

        #endregion

        #region CollectorConfiguration

        private class CollectorConfiguration : IFluentCollectorConfiguration
        {
            private readonly Configure _configureInstance;
            private readonly Type _collectorType;

            public CollectorConfiguration(Configure configureInstance, Type collectorType)
            {
                _configureInstance = configureInstance;
                _collectorType = collectorType;
            }

            IFluentConfiguration IFluentCollectorConfiguration.ForTypesAssignableFrom(IEnumerable<Type> types)
            {
                if (!MappingExists())
                {
                    _configureInstance._container.RegisterTransient<IMethodDataCollector>(_collectorType, _collectorType.FullName);
                    _configureInstance._profilerConfiguration.MethodDataCollectorMappings.AddMapping(new CollectorMapping
                    {
                        CollectorType = _collectorType,
                        ForTypesAssignableFrom = types
                    });
                }

                return _configureInstance;
            }

            IFluentConfiguration IFluentCollectorConfiguration.ForAnyType()
            {
                if (!MappingExists())
                {
                    _configureInstance._container.RegisterTransient<IMethodDataCollector>(_collectorType, _collectorType.FullName);
                    _configureInstance._profilerConfiguration.MethodDataCollectorMappings.AddMapping(new CollectorMapping
                    {
                        CollectorType = _collectorType,
                        ForAnyType = true
                    });
                }

                return _configureInstance;
            }

            IFluentConfiguration IFluentCollectorConfiguration.ForAnyUnmappedType()
            {
                if (!MappingExists())
                {
                    _configureInstance._container.RegisterTransient<IMethodDataCollector>(_collectorType, _collectorType.FullName);
                    _configureInstance._profilerConfiguration.MethodDataCollectorMappings.AddMapping(new CollectorMapping
                    {
                        CollectorType = _collectorType,
                        ForAnyUnmappedType = true
                    });
                }

                return _configureInstance;
            }

            private bool MappingExists()
            {
                if (_configureInstance._profilerConfiguration.MethodDataCollectorMappings.IsCollectorTypeMapped(_collectorType))
                {
                    _configureInstance._profilerErrors.Add(new ProfilerError
                    {
                        Message = string.Format("IMethodDataCollector has already been registered for type {0}.".FormatWith(_collectorType.FullName)),
                        Type = ProfilerErrorType.Configuration,
                    });

                    return true;
                }

                return false;
            }
        }

        #endregion
    }
}
