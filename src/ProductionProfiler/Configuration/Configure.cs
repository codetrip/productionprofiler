
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
        private bool _methodEntryDataCollectorSet;
        private bool _methodExitDataCollectorSet;
        private bool _cacheEngineSet;
        private List<Type> _typesToIntercept;
        private Func<HttpRequest, bool> _requestFilter;
        private ProfilerConfiguration _profilerConfiguration;
        private IContainer _container;
        private readonly List<ProfilerError> _profilerErrors = new List<ProfilerError>();

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

        IFluentConfiguration IFluentConfiguration.InterceptTypes(Type[] typesToIntercept)
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

        public IFluentConfiguration WithCacheEngine<T>() where T : ICacheEngine
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

        public IFluentConfiguration WithMethodEntryDataCollector<T>() where T : IMethodEntryDataCollector
        {
            if (_methodEntryDataCollectorSet)
            {
                _profilerErrors.Add(new ProfilerError
                {
                    Message = string.Format("IMethodEntryDataCollector has already been specified, method entry data collector of type {0} was not registered".FormatWith(typeof(T).FullName)),
                    Type = ProfilerErrorType.Configuration,
                });
            }

            _methodEntryDataCollectorSet = true;
            _container.RegisterTransient<IMethodEntryDataCollector>(typeof(T));
            return this;
        }

        public IFluentConfiguration WithMethodExitDataCollector<T>() where T : IMethodExitDataCollector
        {
            if (_methodExitDataCollectorSet)
            {
                _profilerErrors.Add(new ProfilerError
                {
                    Message = string.Format("IMethodExitDataCollector has already been specified, method exit data collector of type {0} was not registered".FormatWith(typeof(T).FullName)),
                    Type = ProfilerErrorType.Configuration,
                });
            }

            _methodExitDataCollectorSet = true;
            _container.RegisterTransient<IMethodExitDataCollector>(typeof(T));
            return this;
        }

        public IFluentConfiguration WithHttpRequestDataCollector<T>() where T : IHttpRequestDataCollector
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

        public IFluentConfiguration WithHttpResponseDataCollector<T>() where T : IHttpResponseDataCollector
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


        IFluentConfiguration IFluentConfiguration.WithLog4Net(string loggerName)
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

        IFluentConfiguration IFluentConfiguration.WithDataProvider(IPersistenceProvider persistenceProvider)
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
            _container.RegisterTransient<IRequestHandler>(typeof(DeleteProfiledDataByIdRequestHandler), Constants.Handlers.DeleteProfiledRequestDataById);
            _container.RegisterTransient<IRequestHandler>(typeof(DeleteProfiledDataByUrlRequestHandler), Constants.Handlers.DeleteProfiledRequestDataByUrl);
            _container.RegisterTransient<IAddProfiledRequestRequestBinder>(typeof(AddProfiledRequestRequestBinder));
            _container.RegisterTransient<IUpdateProfiledRequestRequestBinder>(typeof(UpdateProfiledRequestRequestBinder));
            _container.RegisterTransient<IRequestProfilingCoordinator>(typeof(RequestProfilingCoordinator));

            if (!_methodEntryDataCollectorSet)
                _container.RegisterTransient<IMethodEntryDataCollector>(typeof(NullMethodEntryDataCollector));
            if (!_methodExitDataCollectorSet)
                _container.RegisterTransient<IMethodExitDataCollector>(typeof(NullMethodExitDataCollector));
            if (!_requestDataCollectorSet)
                _container.RegisterTransient<IHttpRequestDataCollector>(typeof(NullHttpRequestDataCollector));
            if (!_responseDataCollectorSet)
                _container.RegisterTransient<IHttpResponseDataCollector>(typeof(NullHttpResponseDataCollector));
            if (!_cacheEngineSet)
                _container.RegisterTransient<ICacheEngine>(typeof(HttpRuntimeCacheEngine));

            _container.InitialiseForProxyInterception(_typesToIntercept);
        }
    }

    public interface IFluentConfiguration
    {
        IFluentConfiguration InterceptTypes(Type[] typesToIntercept = null);
        IFluentConfiguration RequestFilter(Func<HttpRequest, bool> requestFilter);
        IFluentConfiguration EnableMonitoring(long postThreshold, long getThreshold);
        IFluentConfiguration CaptureExceptions();
        IFluentConfiguration WithLog4Net(string loggerName);
        IFluentConfiguration WithDataProvider(IPersistenceProvider persistenceProvider);
        IFluentConfiguration WithCacheEngine<T>() where T : ICacheEngine;
        IFluentConfiguration WithMethodEntryDataCollector<T>() where T : IMethodEntryDataCollector;
        IFluentConfiguration WithMethodExitDataCollector<T>() where T : IMethodExitDataCollector;
        IFluentConfiguration WithHttpRequestDataCollector<T>() where T : IHttpRequestDataCollector;
        IFluentConfiguration WithHttpResponseDataCollector<T>() where T : IHttpResponseDataCollector;
        void Initialise();
    }
}
