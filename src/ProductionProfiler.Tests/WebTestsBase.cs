
using System.IO;
using Castle.Facilities.FactorySupport;
using Castle.MicroKernel.Releasers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using NUnit.Framework;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Factory;
using ProductionProfiler.Core.Logging;
using ProductionProfiler.IoC.Windsor;
using ProductionProfiler.Persistence.Sql;
using ProductionProfiler.Tests.Components;
using ProductionProfiler.Tests.Mocks;

namespace ProductionProfiler.Tests
{
    public class WebTestsBase
    {
        [TestFixtureSetUp]
        public void SetupMockContext()
        {
            MockHttpContext mockHttpContext = new MockHttpContext(false);
            HttpContextFactory.SetHttpContext(mockHttpContext.Context);
        }

        protected void ConfigureWithCastle()
        {
            var container = new WindsorContainer();
            container.AddFacility<FactorySupportFacility>();
            container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();

            Configure.With(new WindsorProfilerContainer(container))
                .HandleExceptionsVia(e => System.Diagnostics.Trace.Write(e.Format()))
                .Logger(new Log4NetLogger())
                .DataProvider(new SqlPersistenceProvider(new SqlConfiguration("profiler", "profiler", "Profiler")))
                .HttpRequestDataCollector<BasicHttpRequestDataCollector>()
                .HttpResponseDataCollector<BasicHttpResponseDataCollector>()
                .CollectInputOutputMethodDataForTypes(new[] { typeof(IWorkflow) })
                .CacheEngine<NullCacheEngine>()
                .RequestFilter(req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty)
                .CaptureExceptions()
                .CaptureResponse()
                .Initialise();
        }
    }
}
