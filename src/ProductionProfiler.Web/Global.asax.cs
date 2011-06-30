using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Facilities.FactorySupport;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using log4net.Config;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.IoC.Windsor;
using ProductionProfiler.Logging.Log4Net;
using ProductionProfiler.Persistence.SqlServer;
using ProductionProfiler.Web.Controllers;
using ProductionProfiler.Web.Models;
using ProductionProfiler.Web.Profilng;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication, IContainerAccessor
    {
        private static IWindsorContainer _container;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{folder}/{*pathInfo}", new { folder = "profiler" });

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            XmlConfigurator.ConfigureAndWatch(new FileInfo(Server.MapPath("~/bin/Config/log4net.config")));

            try
            {
                _container = new WindsorContainer(new XmlInterpreter());
                _container.AddFacility<FactorySupportFacility>();
                _container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
            }
            catch (ConfigurationErrorsException ex)
            {
                if (ex.Message == "Could not find section 'castle' in the configuration file associated with this domain.")
                    _container = new WindsorContainer();
                else
                    throw;
            }

            ControllerBuilder.Current.SetControllerFactory(typeof(WindsorControllerFactory));

            RegisterDependencies();

            //set up profiler
            Configure.With(new WindsorProfilerContainer(Container))
                .HandleExceptionsVia(e => System.Diagnostics.Trace.Write(e.Format()))
                .Logger(new Log4NetLogger())
                .DataProvider(new SqlPersistenceProvider(new SqlConfiguration("profiler", "profiler", "Profiler")))
                .HttpRequestDataCollector<BasicHttpRequestDataCollector>()
                .HttpResponseDataCollector<BasicHttpResponseDataCollector>()
                .CollectInputOutputMethodDataForTypes(new[] { typeof(IWorkflow) })
                .AddMethodDataCollector<WorkflowMethodDataCollector>()
                    .ForTypesAssignableFrom(new []{typeof(IWorkflow)})
                .CacheEngine<HttpRuntimeCacheEngine>()
                .RequestFilter(req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty)
                .CaptureExceptions()
                .CaptureResponse()
                //.EnableMonitoring(5000, 3000)
                .Initialise();
        }

        public IWindsorContainer Container
        {
            get { return _container; }
        }

        private static void RegisterDependencies()
        {
            _container.Register(
                AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
                    .BasedOn<IController>()
                    .Configure(c => c.LifeStyle.Transient));

            _container.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
                .BasedOn(typeof(IWorkflow<,>))
                    .WithService
                    .FromInterface(typeof(IWorkflow<,>))
                .If(t => true)
                .Configure(c => c.LifeStyle.Transient));
        }
    }
}