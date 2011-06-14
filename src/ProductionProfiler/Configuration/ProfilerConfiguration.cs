
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using log4net;
using log4net.Core;
using ProductionProfiler.Interfaces;
using ProductionProfiler.Interfaces.Entities;
using ProductionProfiler.Interfaces.Exceptions;
using ProductionProfiler.Interfaces.Resources;
using ProductionProfiler.IoC;
using System.Linq;
using ProductionProfiler.Log4Net;
using ProductionProfiler.Profiling;

namespace ProductionProfiler.Configuration
{
    public class ProfilerConfiguration : IFluentConfiguration
    {
        private IList<Type> _typesToIntercept;
        private Func<HttpRequest, bool> _shouldProfile;

        public long GetRequestThreshold { get; set; }
        public long PostRequestThreshold { get; set; }
        public bool Log4NetEnabled { get; set; }
        public Log4NetProfilingAppender ProfilingAppender { get; set; }

        public static IFluentConfiguration With(Type[] typesToIntercept)
        {
            ProfilerConfiguration config = new ProfilerConfiguration();

            config._typesToIntercept = new List<Type>(typesToIntercept);

            //primary marker interface for interception is IWantToBeProfiled so ensure this is included in the types to intercept
            if (!config._typesToIntercept.Any(t => t == typeof(IWantToBeProfiled)))
                config._typesToIntercept.Add(typeof(IWantToBeProfiled));

            //request defaults
            config.GetRequestThreshold = 3000;
            config.PostRequestThreshold = 5000;

            //default to all requests that do not have a file extension (assumes default environment is ASP.NET MVC
            config.ShouldProfileRequest(req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty);
            return config;
        }

        public IFluentConfiguration PostThreshold(long postRequestLengthThreshold)
        {
            PostRequestThreshold = postRequestLengthThreshold;
            return this; 
        }

        public IFluentConfiguration GetThreshold(long getRequestLengthThreshold)
        {
            GetRequestThreshold = getRequestLengthThreshold;
            return this;
        }

        public IFluentConfiguration ShouldProfileRequest(Func<HttpRequest, bool> shouldProfileRequest)
        {
            _shouldProfile = shouldProfileRequest;
            return this;
        }

        public IFluentConfiguration Log4Net()
        {
            Log4NetEnabled = true;

            var profilingLogger = LogManager.Exists(Constants.ProfilingLogger);

            if (profilingLogger == null)
                throw new RequestProfilerConfigurationException("No log4net logger named Profiler was found in the log4net configuration, you must configure a logger named Profiler");

            var appenderAttachable = profilingLogger as IAppenderAttachable;

            if (appenderAttachable != null)
            {
                var profilingAppender = appenderAttachable.GetAppender(Constants.ProfilingAppender) as Log4NetProfilingAppender;

                if (profilingAppender == null)
                    throw new RequestProfilerConfigurationException("No profiling appender was configured, please add an appender named ProfilingAppender of type Log4NetProfilingAppender to the Profiler logger.");

                ProfilingAppender = profilingAppender;
            }

            return this;
        }

        public void Initialise(IWindsorContainer container)
        {
            container.Register(Component.For<IRequestProfiler>()
                .ImplementedBy<Profiling.RequestProfiler>()
                .LifeStyle.Custom<HybridPerWebRequestPerThreadLifestyleManager>());

            container.Register(Component.For<IRequestProfilingManager>()
                .ImplementedBy<RequestProfilingManager>()
                .LifeStyle.Transient);

            container.Register(Component.For<ProfilerConfiguration>()
                .ImplementedBy<ProfilerConfiguration>()
                .LifeStyle.Singleton
                .Instance(this));

            container.Kernel.ProxyFactory.AddInterceptorSelector(new ProfilingInterceptorSelector());

            RequestProfilerContext.Initialise(_shouldProfile, _typesToIntercept, container);
        }
    }

    public interface IFluentConfiguration
    {
        IFluentConfiguration PostThreshold(long postRequestLengthThreshold);
        IFluentConfiguration GetThreshold(long getRequestLengthThreshold);
        IFluentConfiguration ShouldProfileRequest(Func<HttpRequest, bool> shouldProfileRequest);
        IFluentConfiguration Log4Net();
        void Initialise(IWindsorContainer container);
    }
}
