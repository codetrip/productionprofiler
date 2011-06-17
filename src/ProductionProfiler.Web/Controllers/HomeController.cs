
using System;
using System.Web.Mvc;
using log4net;
using ProductionProfiler.Web.Models;

namespace ProductionProfiler.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWorkflow1 _workflow;

        public HomeController(IWorkflow1 workflow)
        {
            _workflow = workflow;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Profile()
        {
            var log = LogManager.GetLogger("Profiler");
            log.Debug("Starting Profile action on Home controller");
            _workflow.Invoke("test");
            log.Debug("Completed Profile action on Home controller");
            return View("index");
        }

    }
}
