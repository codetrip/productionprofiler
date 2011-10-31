using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Facilities.FactorySupport;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using ProductionProfiler.Core.Logging;
using ProductionProfiler.Core.Serialization;
using ProductionProfiler.Persistence.Mongo;
using ProductionProfiler.Persistence.Sql;
using ProductionProfiler.Tests.Components;
using log4net.Config;
using ProductionProfiler.Core.Caching;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.IoC.StructureMap;
using ProductionProfiler.IoC.Windsor;
using ProductionProfiler.Web.Controllers;
using ProductionProfiler.Core.Extensions;
using StructureMap;

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
            Thread.Sleep(10000);
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            XmlConfigurator.ConfigureAndWatch(new FileInfo(Server.MapPath("~/bin/Config/log4net.config")));

            Core.IoC.IContainer container;

            if (ConfigurationManager.AppSettings["IoCContainer"].ToLowerInvariant() == "castle")
            {
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
                RegisterCastleDependencies();
                container = new WindsorProfilerContainer(_container);
            }
            else
            {
                ControllerBuilder.Current.SetControllerFactory(typeof(StructureMapControllerFactory));
                RegisterStructureMapDependencies();
                container = new StructureMapProfilerContainer(ObjectFactory.Container);
            }

            //set up profiler
            Configure.With(container)
                .HandleExceptionsVia(e => System.Diagnostics.Trace.Write(e.Format()))
                .Logger(new Log4NetLogger())
                .DataProvider(new MongoPersistenceProvider("127.0.0.1", 27017))
                .HttpRequestDataCollector<BasicHttpRequestDataCollector>()
                .HttpResponseDataCollector<BasicHttpResponseDataCollector>()
                .TypesToIntercept(new[] { typeof(IWorkflow) })
                .TypesToIgnore(new[] { typeof(IController) })
                .CollectMethodDataForTypes(new[] { typeof(IWorkflow) })
                .AddMethodInvocationDataCollector<WorkflowMethodInvocationDataCollector>()
                    .ForTypesAssignableFrom(new []{typeof(IWorkflow)})
                .CacheEngine<HttpRuntimeCacheEngine>()
                .RequestFilter(req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty)
                .Trigger
                    .BySession()
                .Trigger
                    .ByUrl()
                .Trigger
                    .BySampling()
                        .Every(new TimeSpan(0, 0, 30))
                        .For(new TimeSpan(0, 0, 5))
                        .Enable()
                .Serializer<JsonSerializer>()
                .Initialise();
        }

        public IWindsorContainer Container
        {
            get { return _container; }
        }

        private static void RegisterStructureMapDependencies()
        {
            ObjectFactory.Configure(c => c.Scan(a =>
            {
                a.AssemblyContainingType(typeof(IWorkflow));
                a.With(new DerivedOpenGenericInterfaceConnectionScanner(typeof(IWorkflow<,>)));
            }));

            ObjectFactory.Configure(c => c.Scan(a =>
            {
                a.TheCallingAssembly();
                a.AddAllTypesOf(typeof(IController));
            }));
        }

        private static void RegisterCastleDependencies()
        {
            _container.Register(
                AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
                    .BasedOn<IController>()
                    .Configure(c => c.LifeStyle.Transient));

            _container.Register(AllTypes.FromAssembly(typeof(IWorkflow).Assembly)
                .BasedOn(typeof(IWorkflow<,>))
                    .WithService
                    .FromInterface(typeof(IWorkflow<,>))
                .If(t => true)
                .Configure(c => c.LifeStyle.Transient));
        }
    }
}