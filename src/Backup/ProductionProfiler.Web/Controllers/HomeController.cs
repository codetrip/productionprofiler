
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ProductionProfiler.Tests.Components;
using log4net;

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
            _workflow.Invoke(new TestRequest
            {
                Id = 12,
                Name = "Workflow One",
                DictItems = new Dictionary<Guid, TestClass>
                {
                    {Guid.NewGuid(), new TestClass()},
                    {Guid.NewGuid(), new TestClass
                                        {
                                            Id = 321,
                                            Name = "Test 1",
                                            Parent = new TestClass
                                            {
                                                Id = 432432,
                                                Name = "Bob"
                                            }
                                        }}
                },
                Items = new List<string>
                {
                    "Bob",
                    "The",
                    "Builder"
                }
            });
            log.Debug("Completed Profile action on Home controller");
            return View("index");
        }

    }
}
