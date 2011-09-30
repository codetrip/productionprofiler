
using System;
using System.Collections.Generic;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Factory;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.RequestTiming;

namespace ProductionProfiler.Core.Profiling
{
    public class ProfilerContext
    {
        private const string ProfilingContextKey = "profiling";

        private static bool _initialised;
        private static IContainer _container;
        private static ProfilerConfiguration _configuration;
        private static PersistenceWorkerQueue _persistenceWorkerQueue;

        internal static void Initialise(IContainer container, ProfilerConfiguration configuration)
        {
            _configuration = configuration;
            _container = container;
            _initialised = true;
            _persistenceWorkerQueue = new PersistenceWorkerQueue(new Dictionary<Type, Action<IAsyncPersistable>>
            {
                {typeof(ProfiledRequestData), PersistProfiledRequestData},
                {typeof(ProfiledResponse), PersistProfiledResponse},
                {typeof(TimedRequest), PersistTimedRequest},
            });
        }

        private static void PersistTimedRequest(IAsyncPersistable data)
        {
            if (data is TimedRequest)
                Container.Resolve<IProfilerRepository>().SaveTimedRequest(data as TimedRequest);
        }

        private static void PersistProfiledRequestData(IAsyncPersistable data)
        {
            if (data is ProfiledRequestData)
                Container.Resolve<IProfilerRepository>().SaveProfiledRequestData(data as ProfiledRequestData);
        }

        private static void PersistProfiledResponse(IAsyncPersistable data)
        {
            if (data is ProfiledResponse)
                Container.Resolve<IProfilerRepository>().SaveResponse(data as ProfiledResponse);
        }

        public static ProfilerConfiguration Configuration
        {
            get { return _configuration; }
        }

        public static IContainer Container
        {
            get { return _container; }
        }

        public static bool Initialised
        {
            get { return _initialised; }
        }

        public static void EnqueueForPersistence(IAsyncPersistable data)
        {
            _persistenceWorkerQueue.Enqueue(data);
        }

        public static bool Profiling
        {
            get
            {
                var httpContext = HttpContextFactory.GetHttpContext();
                if (httpContext == null)
                    return false;

                object val = httpContext.Items[ProfilingContextKey];
                return val != null && (bool) val;
            }
            set
            {
                var httpContext = HttpContextFactory.GetHttpContext();
                if (httpContext != null)
                    httpContext.Items[ProfilingContextKey] = value;
            }
        }

        public static IRequestProfiler Profiler
        {
            get { return _container.Resolve<IRequestProfiler>(); }
        }

        public static IRequestTimer RequestTimer
        {
            get { return _container.Resolve<IRequestTimer>(); }
        }
    }
}