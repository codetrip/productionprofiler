﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ProductionProfiler.Core.Binding;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Handlers;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Logging;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Resources;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Serialization;
using System.Linq;

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
        private Func<HttpRequest, bool> _requestFilter;
        private Action<Exception> _exceptionHandler;
        private IContainer _container;
        private ProfilerConfiguration _profilerConfiguration;
        private Func<HttpContext, bool> _authorisedForManagement;

        public static IFluentExceptionConfiguration With(IContainer container)
        {
            Configure config = new Configure
            {
                _profilerConfiguration = new ProfilerConfiguration(),
                _requestFilter = req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty,
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
                _exceptionHandler(new ProfilerConfigurationException("IProfilerCacheEngine has already been specified, cache engine of type {0} was not registered".FormatWith(typeof(T).FullName)));
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
                _exceptionHandler(new ProfilerConfigurationException(string.Format("ISerializer has already been specified, serializer of type {0} was not registered".FormatWith(typeof(T).FullName))));
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
            try
            {
                _container.RegisterTransient<IProfilerRepository>(persistenceProvider.RepositoryType);
                persistenceProvider.RegisterDependentComponents(_container);
                persistenceProvider.Initialise();
            }
            catch (Exception e)
            {
                _exceptionHandler(e);
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

        public IFluentConfiguration Authorize(Func<HttpContext, bool> authorisedForManagement)
        {
            _authorisedForManagement = authorisedForManagement;
            return this;
        }

        public IFluentConfiguration CollectInputOutputMethodDataForTypes(IEnumerable<Type> typesToCollectInputOutputDataFor)
        {
            _profilerConfiguration.MethodDataCollectorMappings.InputOutputMethodDataTypes = typesToCollectInputOutputDataFor.ToList();
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
                _exceptionHandler(new ProfilerConfigurationException(string.Format("IHttpRequestDataCollector has already been specified, http request data collector of type {0} was not registered".FormatWith(typeof(T).FullName))));
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
                _exceptionHandler(new ProfilerConfigurationException(string.Format("IHttpResponseDataCollector has already been specified, http response data collector of type {0} was not registered".FormatWith(typeof(T).FullName))));
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
                RequestProfilerContext.Initialise(_requestFilter, _container, _authorisedForManagement, _exceptionHandler);
            }
            catch (Exception e)
            {
                _exceptionHandler(e);
            }
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
            _container.RegisterTransient<IMethodInputOutputDataCollector>(typeof(MethodInputOutputDataCollector));

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
                    _configureInstance._exceptionHandler(new ProfilerConfigurationException(string.Format("IMethodDataCollector has already been registered for type {0}.".FormatWith(_collectorType.FullName))));
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

                _configureInstance._exceptionHandler = exceptionHandler;
                return _configureInstance;
            }
        }

        #endregion
    }
}
