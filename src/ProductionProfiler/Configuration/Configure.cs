
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ProductionProfiler.Core.Auditing;
using ProductionProfiler.Core.Binding;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Coordinators;
using ProductionProfiler.Core.Handlers;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Logging;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Serialization;
using System.Linq;
using ProductionProfiler.Core.Web;

namespace ProductionProfiler.Core.Configuration
{
    public class Configure : IFluentConfiguration
    {
        private bool _requestDataCollectorSet;
        private bool _responseDataCollectorSet;
        private bool _cacheEngineSet;
        private bool _serializerSet;
        private bool _loggerSet;
        private IEnumerable<Type> _typesToIntercept;
        private IEnumerable<Type> _typesToIgnore;
        private IContainer _container;
        private ProfilerConfiguration _profilerConfiguration;

        public static IFluentExceptionConfiguration With(IContainer container)
        {
            Configure config = new Configure
            {
                _profilerConfiguration = new ProfilerConfiguration
                {
                    RequestFilter = req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty
                },
                _container = container
            };

            return new ExceptionConfiguration(config);
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
            _profilerConfiguration.RequestFilter = requestFilter;
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
            _profilerConfiguration.ResponseFilter = responseFilterConstructor;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.CaptureResponse()
        {
            _profilerConfiguration.CaptureResponse = true;
            _profilerConfiguration.ResponseFilter = context => new StoreResponseFilter(context.Response.Filter);
            return this;
        }

        IFluentConfiguration IFluentConfiguration.CacheEngine<T>()
        {
            if (_cacheEngineSet)
            {
                _profilerConfiguration.ReportException(new ProfilerConfigurationException("IProfilerCacheEngine has already been specified, cache engine of type {0} was not registered".FormatWith(typeof(T).FullName)));
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
                _profilerConfiguration.ReportException(new ProfilerConfigurationException(string.Format("ISerializer has already been specified, serializer of type {0} was not registered".FormatWith(typeof(T).FullName))));
                return this;
            }

            _serializerSet = true;
            _container.RegisterTransient<ISerializer>(typeof(T));
            return this;
        }

        IFluentConfiguration IFluentConfiguration.DataProvider(IPersistenceProvider persistenceProvider)
        {
            try
            {
                _container.RegisterTransient<IProfilerRepository>(persistenceProvider.RepositoryType);
                persistenceProvider.RegisterDependentComponents(_container);
                persistenceProvider.Initialise();
            }
            catch (Exception e)
            {
                _profilerConfiguration.ReportException(e);
            }
            
            return this;
        }

        IFluentConfiguration IFluentConfiguration.Logger(ILogger logger)
        {
            logger.Initialise();
            _loggerSet = true;
            _container.RegisterTransient<ILogger>(logger.GetType());
            return this;
        }

        #region Collectors

        public IFluentConfiguration AuthoriseManagement(Func<HttpContext, bool> authorisedForManagement)
        {
            _profilerConfiguration.AuthoriseManagement = authorisedForManagement;
            return this;
        }

        public IFluentConfiguration AuthoriseSession(Func<string, bool> authoriseSession)
        {
            _profilerConfiguration.AuthoriseSession = authoriseSession;
            return this;
        }

        public IFluentConfiguration CollectInputOutputMethodDataForTypes(IEnumerable<Type> typesToCollectInputOutputDataFor)
        {
            _profilerConfiguration.MethodDataCollectorMappings.InputOutputMethodDataTypes = typesToCollectInputOutputDataFor.ToList();
            return this;
        }

        public IFluentCoordinatorConfiguration Coordinators
        {
            get { return new FluentCoordinatorConfiguration(this); }
        }

        IFluentCollectorConfiguration IFluentConfiguration.AddMethodDataCollector<T>()
        {
            return new CollectorConfiguration(this, typeof(T));
        }

        IFluentConfiguration IFluentConfiguration.HttpRequestDataCollector<T>()
        {
            if (_requestDataCollectorSet)
            {
                _profilerConfiguration.ReportException(new ProfilerConfigurationException(string.Format("IHttpRequestDataCollector has already been specified, http request data collector of type {0} was not registered".FormatWith(typeof(T).FullName))));
                return this;
            }

            _requestDataCollectorSet = true;
            _container.RegisterTransient<IHttpRequestDataCollector>(typeof(T));
            return this;
        }

        IFluentConfiguration IFluentConfiguration.HttpResponseDataCollector<T>()
        {
            if (_responseDataCollectorSet)
            {
                _profilerConfiguration.ReportException(new ProfilerConfigurationException(string.Format("IHttpResponseDataCollector has already been specified, http response data collector of type {0} was not registered".FormatWith(typeof(T).FullName))));
                return this;
            }

            _responseDataCollectorSet = true;
            _container.RegisterTransient<IHttpResponseDataCollector>(typeof(T));
            return this;
        }

        #endregion

        #region Initialise & Dependencies

        void IFluentConfiguration.Initialise()
        {
            try
            {
                RegisterDependencies();
                ProfilerContext.Initialise(_container, _profilerConfiguration);
            }
            catch (Exception e)
            {
                _profilerConfiguration.ReportException(e);
            }
        }

        private void RegisterDependencies()
        {
            _container.RegisterPerWebRequest<IRequestProfiler>(typeof(RequestProfiler));
            _container.RegisterSingletonInstance(_profilerConfiguration);
            _container.RegisterTransient<IRequestHandler>(typeof(UpdateUrlToProfileHandler), Constants.Handlers.UpdateUrlToProfile);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewResultsRequestHandler), Constants.Handlers.Results);
            _container.RegisterTransient<IRequestHandler>(typeof(AddUrlToProfileHandler), Constants.Handlers.AddUrlToProfile);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewUrlToProfilesHandler), Constants.Handlers.ViewUrlToProfiles);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewResponseRequestHandler), Constants.Handlers.ViewResponse);
            _container.RegisterTransient<IRequestHandler>(typeof(DeleteProfiledDataByIdRequestHandler), Constants.Handlers.DeleteUrlToProfileDataById);
            _container.RegisterTransient<IRequestHandler>(typeof(DeleteProfiledDataByUrlRequestHandler), Constants.Handlers.DeleteUrlToProfileDataByUrl);
            _container.RegisterTransient<IAddUrlToProfileRequestBinder>(typeof(AddUrlToProfileRequestBinder));
            _container.RegisterTransient<IUpdateUrlToProfileRequestBinder>(typeof(UpdateUrlToProfileRequestBinder));
            _container.RegisterTransient<IMethodInputOutputDataCollector>(typeof(MethodInputOutputDataCollector));
            _container.RegisterTransient<IComponentAuditor>(typeof(Log4NetComponentAuditor));
            _container.RegisterSingleton<ICookieManager>(typeof(CookieManager));

            if (!_requestDataCollectorSet)
                _container.RegisterTransient<IHttpRequestDataCollector>(typeof(NullHttpRequestDataCollector));
            if (!_responseDataCollectorSet)
                _container.RegisterTransient<IHttpResponseDataCollector>(typeof(NullHttpResponseDataCollector));
            if (!_cacheEngineSet)
                _container.RegisterTransient<IProfilerCacheEngine>(typeof(HttpRuntimeCacheEngine));
            if (!_serializerSet)
                _container.RegisterTransient<ISerializer>(typeof(JsonSerializer));
            if (!_loggerSet)
                _container.RegisterTransient<ILogger>(typeof(DefaultLogger));

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
                    _configureInstance._profilerConfiguration.ReportException(new ProfilerConfigurationException(string.Format("IMethodDataCollector has already been registered for type {0}.".FormatWith(_collectorType.FullName))));
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region ExceptionConfiguration

        private class ExceptionConfiguration : IFluentExceptionConfiguration
        {
            private readonly Configure _configureInstance;

            public ExceptionConfiguration(Configure configureInstance)
            {
                _configureInstance = configureInstance;
            }

            public IFluentConfiguration HandleExceptionsVia(Action<Exception> exceptionHandler)
            {
                //if no handler supplied just output to debug trace
                if (exceptionHandler == null)
                    exceptionHandler = e => System.Diagnostics.Trace.Write(e.Format());

                _configureInstance._profilerConfiguration.ReportException = exceptionHandler;
                return _configureInstance;
            }
        }

        #endregion

        #region CoordinatorConfiguration

        private class FluentCoordinatorConfiguration : IFluentCoordinatorConfiguration
        {
            private readonly Configure _configureInstance;

            public FluentCoordinatorConfiguration(Configure configureInstance)
            {
                _configureInstance = configureInstance;
            }

            public IFluentConfiguration Register<T>() where T : IProfilingCoordinator
            {
                _configureInstance._container.RegisterTransient<IProfilingCoordinator>(typeof(T), typeof(T).Name);
                return _configureInstance;
            }

            public IFluentCoordinatorConfiguration Url()
            {
                _configureInstance._container.RegisterTransient<IProfilingCoordinator>(typeof(UrlCoordinator), typeof(UrlCoordinator).Name);
                return this;
            }

            public IFluentCoordinatorConfiguration Session()
            {
                _configureInstance._container.RegisterTransient<IProfilingCoordinator>(typeof(SessionCoordinator), typeof(SessionCoordinator).Name);
                return this;
            }

            public IFluentCoordinatorConfiguration Sampling()
            {
                _configureInstance._container.RegisterTransient<IProfilingCoordinator>(typeof(SamplingCoordinator), typeof(SamplingCoordinator).Name);
                return this;
            }

            public IFluentConfiguration Add()
            {
                return _configureInstance;
            }
        }

        #endregion
    }
}
