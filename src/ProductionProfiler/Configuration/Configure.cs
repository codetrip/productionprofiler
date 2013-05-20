
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ProductionProfiler.Core.Auditing;
using ProductionProfiler.Core.Binding;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Handlers;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Logging;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling;
using ProductionProfiler.Core.Profiling.Triggers;
using ProductionProfiler.Core.RequestTiming;
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
		private bool _auditingEnabled = false;
        private IEnumerable<Type> _typesToIntercept;
        private IEnumerable<Type> _typesToIgnore;
        private IContainer _container;
        private ProfilerConfiguration _profilerConfiguration;

        public static IFluentExceptionConfiguration With(IContainer container)
        {
            var config = new Configure
            {
                _profilerConfiguration = new ProfilerConfiguration
                {
                    RequestFilter = req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty,
                },
                _container = container
            };

            //enabled by default
            config._profilerConfiguration.Settings.Add(ProfilerConfiguration.SettingKeys.ProfilerEnabled, "true");
            config._container.RegisterTransient<IProfilingTrigger>(typeof(UrlBasedProfilingTrigger), typeof(UrlBasedProfilingTrigger).Name);
            config._container.RegisterTransient<IProfilingTrigger>(typeof(SessionBasedProfilingTrigger), typeof(SessionBasedProfilingTrigger).Name);
            config._container.RegisterTransient<IProfilingTrigger>(typeof(SampleBasedProfilingTrigger), typeof(SampleBasedProfilingTrigger).Name);
            config._profilerConfiguration.Settings.Add(ProfilerConfiguration.SettingKeys.UrlTriggerEnabled, "false");
            config._profilerConfiguration.Settings.Add(ProfilerConfiguration.SettingKeys.SessionTriggerEnabled, "false");
            config._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.SamplingFrequency] = new TimeSpan(1, 0, 0).ToString();
            config._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.SamplingPeriod] = new TimeSpan(0, 0, 10).ToString();
            config._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.SamplingTriggerEnabled] = "false";
            config._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.LongRequestThresholdMs] = "2000";
            config._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.TimeAllRequests] = "false";
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

        IFluentConfiguration IFluentConfiguration.TimeAllRequests(int longRequestThresholdMs)
        {
            _profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.TimeAllRequests] = "true";
            _profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.LongRequestThresholdMs] = longRequestThresholdMs.ToString();
            return this;
        }

        IFluentConfiguration IFluentConfiguration.DoNotTimeAllRequests()
        {
            _profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.TimeAllRequests] = "false";
            return this;
        }

	    public IFluentConfiguration EnableAuditing()
	    {
		    _auditingEnabled = true;
			_container.RegisterTransient<IComponentAuditor>(typeof(Log4NetComponentAuditor));
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

        public IFluentConfiguration CollectMethodDataForTypes(IEnumerable<Type> typesToCollectInputOutputDataFor)
        {
            _profilerConfiguration.DataCollectorMappings.CollectMethodDataForTypes = typesToCollectInputOutputDataFor.ToList();
            return this;
        }

        public IFluentProfilingTriggerConfiguration Trigger
        {
            get { return new FluentProfilingTriggerConfiguration(this); }
        }

        IFluentCollectorConfiguration IFluentConfiguration.AddMethodInvocationDataCollector<T>()
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
            _container.RegisterPerWebRequest<IRequestTimer>(typeof(RequestTimer));
            _container.RegisterSingletonInstance(_profilerConfiguration);

            _container.RegisterTransient<IRequestHandler>(typeof(UpdateUrlToProfileHandler), Constants.Handlers.UpdateUrlToProfile);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewResultsRequestHandler), Constants.Handlers.Results);
            _container.RegisterTransient<IRequestHandler>(typeof(AddUrlToProfileHandler), Constants.Handlers.AddUrlToProfile);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewUrlToProfilesHandler), Constants.Handlers.ViewUrlToProfiles);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewResponseRequestHandler), Constants.Handlers.ViewResponse);
            _container.RegisterTransient<IRequestHandler>(typeof(DeleteProfiledDataByIdRequestHandler), Constants.Handlers.DeleteUrlToProfileDataById);
            _container.RegisterTransient<IRequestHandler>(typeof(DeleteProfiledDataByUrlRequestHandler), Constants.Handlers.DeleteUrlToProfileDataByUrl);
            _container.RegisterTransient<IRequestHandler>(typeof(ConfigurationOverrideHandler), Constants.Handlers.ConfigurationOverride);
            _container.RegisterTransient<IRequestHandler>(typeof(ViewLongRequestsHandler), Constants.Handlers.LongRequests);
            _container.RegisterTransient<IRequestHandler>(typeof(ClearLongRequestsHandler), Constants.Handlers.ClearLongRequests);

            _container.RegisterTransient<IAddUrlToProfileRequestBinder>(typeof(AddUrlToProfileRequestBinder));
            _container.RegisterTransient<IUpdateUrlToProfileRequestBinder>(typeof(UpdateUrlToProfileRequestBinder));
            _container.RegisterTransient<IMethodDataCollector>(typeof(MethodDataCollector));
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
			if (!_auditingEnabled)
				_container.RegisterTransient<IComponentAuditor>(typeof(NullComponentAuditor));

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
                    _configureInstance._container.RegisterTransient<IMethodInvocationDataCollector>(_collectorType, _collectorType.FullName);
                    _configureInstance._profilerConfiguration.DataCollectorMappings.AddMapping(new CollectorMapping
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
                    _configureInstance._container.RegisterTransient<IMethodInvocationDataCollector>(_collectorType, _collectorType.FullName);
                    _configureInstance._profilerConfiguration.DataCollectorMappings.AddMapping(new CollectorMapping
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
                    _configureInstance._container.RegisterTransient<IMethodInvocationDataCollector>(_collectorType, _collectorType.FullName);
                    _configureInstance._profilerConfiguration.DataCollectorMappings.AddMapping(new CollectorMapping
                    {
                        CollectorType = _collectorType,
                        ForAnyUnmappedType = true
                    });
                }

                return _configureInstance;
            }

            private bool MappingExists()
            {
                if (_configureInstance._profilerConfiguration.DataCollectorMappings.IsCollectorTypeMapped(_collectorType))
                {
                    _configureInstance._profilerConfiguration.ReportException(new ProfilerConfigurationException(string.Format("IMethodInvocationDataCollector has already been registered for type {0}.".FormatWith(_collectorType.FullName))));
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

        #region Profiling Trigger Configuration

        private class FluentProfilingTriggerConfiguration : IFluentProfilingTriggerConfiguration
        {
            private readonly Configure _configureInstance;

            public FluentProfilingTriggerConfiguration(Configure configureInstance)
            {
                _configureInstance = configureInstance;
            }

            public IFluentConfiguration ByCustomTrigger<T>() where T : IProfilingTrigger
            {
                _configureInstance._container.RegisterTransient<IProfilingTrigger>(typeof(T), typeof(T).Name);
                return _configureInstance;
            }

            public IFluentConfiguration ByUrl()
            {
                _configureInstance._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.UrlTriggerEnabled] = "true";
                return _configureInstance;
            }

            public IFluentConfiguration BySession()
            {
                _configureInstance._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.SessionTriggerEnabled] = "true";
                return _configureInstance;
            }

            public IFluentSampleProfilingTriggerConfiguration BySampling()
            {
                return new FluentSampleProfilingTriggerConfiguration(_configureInstance);
            }
        }

        public class FluentSampleProfilingTriggerConfiguration : IFluentSampleProfilingTriggerConfiguration
        {
            private readonly Configure _configureInstance;

            public FluentSampleProfilingTriggerConfiguration(Configure configureInstance)
            {
                _configureInstance = configureInstance;
            }

            public IFluentSampleProfilingTriggerConfiguration Every(TimeSpan frequency)
            {
                _configureInstance._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.SamplingFrequency] = frequency.ToString();
                return this;
            }

            public IFluentSampleProfilingTriggerConfiguration For(TimeSpan period)
            {
                _configureInstance._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.SamplingPeriod] = period.ToString();
                return this;
            }

            public IFluentConfiguration Enable()
            {
                _configureInstance._profilerConfiguration.Settings[ProfilerConfiguration.SettingKeys.SamplingTriggerEnabled] = "true";
                return _configureInstance;
            }
        }

        #endregion
    }
}
