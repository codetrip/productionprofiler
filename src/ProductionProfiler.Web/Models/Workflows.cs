
using System;
using log4net;

namespace ProductionProfiler.Web.Models
{
    public class TestWorkflow1 : IWorkflow1
    {
        private readonly IWorkflow2 _workflow2;

        public TestWorkflow1(IWorkflow2 workflow2)
        {
            _workflow2 = workflow2;
        }

        public string Invoke(string request)
        {
            var log = LogManager.GetLogger("Profiler");
            log.Debug("Started workflow 1");

            try
            {
                throw new Exception("test");
            }
            catch (Exception)
            {
                
            }

            _workflow2.Invoke("test");
            log.Debug("Completed workflow 1");
            return "done";
        }
    }

    public class TestWorkflow2 : IWorkflow2
    {
        private readonly IWorkflow3 _workflow3;

        public TestWorkflow2(IWorkflow3 workflow3)
        {
            _workflow3 = workflow3;
        }

        public string Invoke(string request)
        {
            var log = LogManager.GetLogger("Profiler");
            log.Debug("Started workflow 2");
            _workflow3.Invoke("test");
            log.Debug("Completed workflow 2");
            return "done";
        }
    }

    public class TestWorkflow3 : IWorkflow3
    {
        public string Invoke(string request)
        {
            var log = LogManager.GetLogger("Profiler");
            log.Debug("Started workflow 3");
            log.Error(new Exception("This is a test exception"));
            log.Debug("Completed workflow 3");
            return "done";
        }
    }

    public interface IWorkflow1 : IWorkflow<string, string>
    { }

    public interface IWorkflow2 : IWorkflow<string, string>
    { }

    public interface IWorkflow3 : IWorkflow<string, string>
    { }
}