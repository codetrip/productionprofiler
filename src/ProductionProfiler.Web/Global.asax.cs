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
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.IoC.Windsor;
using ProductionProfiler.Persistence.Mongo;
using ProductionProfiler.Web.Controllers;
using ProductionProfiler.Web.Models;

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

            //set up request profiler
            Configure.With(new WindsorProfilerContainer(Container))
                .GetThreshold(2500)
                .PostThreshold(3500)
                .Log4Net("Profiler")
                .WithDataProvider(new MongoDataProvider("127.0.0.1", 27017))
                .RequestFilter(req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty)
                .CaptureExceptions()
                .EnableMonitoring()
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