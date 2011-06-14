
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using log4net;
using log4net.Core;
using ProductionProfiler.DataAccess;
using ProductionProfiler.Handlers;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Exceptions;
using ProductionProfiler.Interfaces.Resources;
using ProductionProfiler.IoC;
using ProductionProfiler.Log4Net;
using ProductionProfiler.Mongo;
using ProductionProfiler.Profiling;

namespace ProductionProfiler.Configuration
{
    public class Configure : IFluentConfiguration
    {
        private List<Type> _typesToIntercept;
        private Func<HttpRequest, bool> _requestFilter;
        private ProfilerConfiguration _profilerConfiguration;
        private IWindsorContainer _container;
        private bool _dataProviderInitialised;

        public static IFluentConfiguration With(IWindsorContainer container)
        {
            Configure config = new Configure
            {
                _typesToIntercept = new List<Type>(new[] {typeof (IWantToBeProfiled)}),
                _profilerConfiguration = new ProfilerConfiguration
                {
                    GetRequestThreshold = 3000,
                    Log4NetEnabled = false,
                    PostRequestThreshold = 5000
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

        IFluentConfiguration IFluentConfiguration.Log4Net(string loggerName)
        {
            _profilerConfiguration.Log4NetEnabled = true;

            var profilingLogger = LogManager.Exists(loggerName);

            if (profilingLogger == null)
                throw new RequestProfilerConfigurationException(string.Format("No log4net logger named {0} was found in the log4net configuration", loggerName));

            var appenderAttachable = profilingLogger as IAppenderAttachable;

            if (appenderAttachable != null)
            {
                var profilingAppender = new Log4NetProfilingAppender
                {
                    Threshold = Level.Debug,
                    Name = Constants.ProfilingAppender
                };

                appenderAttachable.AddAppender(profilingAppender);

                _profilerConfiguration.ProfilingAppender = profilingAppender;
            }

            return this;
        }

        IFluentConfiguration IFluentConfiguration.SqlServer(string connectionString)
        {
            if (_dataProviderInitialised)
                throw new RequestProfilerConfigurationException("You have already specified a data provider, you cannot specify more than one provider.");

            _dataProviderInitialised = true;
            return this;
        }

        IFluentConfiguration IFluentConfiguration.Mongo(string server, int port)
        {
            if (_dataProviderInitialised)
                throw new RequestProfilerConfigurationException("You have already specified a data provider, you cannot specify more than one provider.");

            _dataProviderInitialised = true;
            MongoConfiguration mongoConfiguration = new MongoConfiguration(server, port.ToString());

            _container.Register(Component.For<MongoConfiguration>()
                .LifeStyle.Singleton
                .Instance(mongoConfiguration));

            _container.Register(Component.For<IProfiledRequestRepository>()
                .ImplementedBy<ProfiledRequestMongoRepository>()
                .LifeStyle.Transient);

            _container.Register(Component.For<IProfiledRequestDataRepository>()
                .ImplementedBy<ProfiledRequestDataMongoRepository>()
                .LifeStyle.Transient);

            return this;
        }

        IFluentConfiguration IFluentConfiguration.CustomDataProvider(ICustomDataProvider customProvider)
        {
            if (_dataProviderInitialised)
                throw new RequestProfilerConfigurationException("You have already specified a data provider, you cannot specify more than one provider.");

            _dataProviderInitialised = true;
            _container.Register(Component.For<IProfiledRequestRepository>()
                .ImplementedBy(customProvider.ProfiledRequestRepository)
                .LifeStyle.Transient);

            _container.Register(Component.For<IProfiledRequestDataRepository>()
                .ImplementedBy(customProvider.ProfiledRequestDataRepositoryType)
                .LifeStyle.Transient);

            customProvider.RegisterDependentComponents(_container);

            return this;
        }

        void IFluentConfiguration.Initialise()
        {
            _container.Register(Component.For<IRequestProfiler>()
                .ImplementedBy<RequestProfiler>()
                .LifeStyle.Custom<HybridPerWebRequestPerThreadLifestyleManager>());

            _container.Register(Component.For<IRequestProfilingCoordinator>()
                .ImplementedBy<RequestProfilingCoordinator>()
                .LifeStyle.Transient);

            _container.Register(Component.For<ProfilerConfiguration>()
                .LifeStyle.Singleton
                .Instance(_profilerConfiguration));

            _container.Register(Component.For<IRequestHandler>()
                .LifeStyle.Transient
                .Named("profiledrequests")
                .ImplementedBy<ProfiledRequestsRequestHandler>());

            _container.Register(Component.For<IRequestHandler>()
                .LifeStyle.Transient
                .Named("results")
                .ImplementedBy<ProfiledRequestResultsRequestHandler>());

            _container.Kernel.ProxyFactory.AddInterceptorSelector(new ProfilingInterceptorSelector(_typesToIntercept));

            RequestProfilerContext.Initialise(_requestFilter, _container);
        }
    }

    public interface IFluentConfiguration
    {
        IFluentConfiguration PostThreshold(long postRequestLengthThreshold);
        IFluentConfiguration GetThreshold(long getRequestLengthThreshold);
        IFluentConfiguration InterceptTypes(Type[] typesToIntercept);
        IFluentConfiguration RequestFilter(Func<HttpRequest, bool> requestFilter);
        IFluentConfiguration Log4Net(string loggerName);
        IFluentConfiguration SqlServer(string connectionString);
        IFluentConfiguration Mongo(string server, int port);
        IFluentConfiguration CustomDataProvider(ICustomDataProvider customProvider);
        void Initialise();
    }
}
