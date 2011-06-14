using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using log4net.Config;
using ProductionProfiler.Configuration;
using ProductionProfiler.Web.Models;

namespace ProductionProfiler.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication, IContainerAccessor
    {
        private IWindsorContainer _container;
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

            XmlConfigurator.ConfigureAndWatch(new FileInfo(Server.MapPath("bin/Config/log4net.config")));

            try
            {
                _container = new WindsorContainer(new XmlInterpreter());
            }
            catch (ConfigurationErrorsException ex)
            {
                if (ex.Message == "Could not find section 'castle' in the configuration file associated with this domain.")
                    _container = new WindsorContainer();
                else
                    throw;
            }

            //set up request profiler
            Configure.With(Container)
                .GetThreshold(2500)
                .PostThreshold(3500)
                .Log4Net("Profiler")
                .Mongo("127.0.0.1", 27017)
                .RequestFilter(req => Path.GetExtension(req.Url.AbsolutePath) == string.Empty)
                .InterceptTypes(new []{typeof(IWorkflow)})
                .Initialise();
        }

        public IWindsorContainer Container
        {
            get { return _container; }
        }
    }
}