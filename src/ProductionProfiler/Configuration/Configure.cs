
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using ProductionProfiler.Binders;
using ProductionProfiler.Handlers;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Resources;
using ProductionProfiler.IoC;
using ProductionProfiler.Log4Net;
using ProductionProfiler.Profiling;

namespace ProductionProfiler.Configuration
{
    public class Configure : IFluentConfiguration
    {
        private List<Type> _typesToIntercept;
        private Func<HttpRequest, bool> _requestFilter;
        private ProfilerConfiguration _profilerConfiguration;
        private IContainer _container;
        private List<ProfilerError> _profilerErrors = new List<ProfilerError>();
        

        public static IFluentConfiguration With(IContainer container)
        {
            Configure config = new Configure
            {
                _typesToIntercept = new List<Type>(new[] {typeof (IWantToBeProfiled)}),
                _profilerConfiguration = new ProfilerConfiguration
                {
                    GetRequestThreshold = 3000,
                    Log4NetEnabled = false,
                    PostRequestThreshold = 5000,
                    MonitoringEnabled = false
                },
                _requestFilter = req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty,
                _container = container
            };

            return config;
        }

        IFluentConfiguration IFluentConfiguration.PostThreshold(long postRequestLengthThreshold)
        {
            _profilerConfiguration.PostRequestThreshold = postRequestLengthThreshold;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.GetThreshold(long getRequestLengthThreshold)
        {
            _profilerConfiguration.GetRequestThreshold = getRequestLengthThreshold;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.InterceptTypes(Type[] typesToIntercept)
        {
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

        IFluentConfiguration IFluentConfiguration.Log4Net(string loggerName)
        {
            var profilingLogger = LogManager.Exists(loggerName);

            if (profilingLogger == null)
            {
                _profilerErrors.Add(new ProfilerError()
                                        {
                                            Message =
                                                string.Format(
                                                    "No log4net logger named {0} was found in the log4net configuration",
                                                    loggerName),
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
        
        public IFluentConfiguration EnableMonitoring()
        {
            _profilerConfiguration.MonitoringEnabled = true;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.WithDataProvider(IDataProvider dataProvider)
        {
            _container.RegisterTransient<IProfilerRepository>(dataProvider.RepositoryType);
            dataProvider.RegisterDependentComponents(_container);

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
            _container.RegisterTransient<IAddProfiledRequestModelBinder>(typeof(AddProfiledRequestModelBinder));
            _container.RegisterTransient<IUpdateProfiledRequestModelBinder>(typeof(UpdateProfiledRequestModelBinder));
            _container.RegisterTransient<IRequestProfilingCoordinator>(typeof(RequestProfilingCoordinator));
            _container.RegisterTransient<RequestProfilingInterceptor>(typeof(RequestProfilingInterceptor));
            _container.InitialiseForProxyInterception(_typesToIntercept);
        }
    }

    public interface IFluentConfiguration
    {
        IFluentConfiguration PostThreshold(long postRequestLengthThreshold);
        IFluentConfiguration GetThreshold(long getRequestLengthThreshold);
        IFluentConfiguration InterceptTypes(Type[] typesToIntercept);
        IFluentConfiguration RequestFilter(Func<HttpRequest, bool> requestFilter);
        IFluentConfiguration Log4Net(string loggerName);
        IFluentConfiguration EnableMonitoring();
        IFluentConfiguration WithDataProvider(IDataProvider dataProvider);
        void Initialise();
        IFluentConfiguration CaptureExceptions();
    }
}
