using System;
using System.Collections.Generic;
using log4net;

namespace ProductionProfiler.Tests.Components
{
    public class TestWorkflow1 : IWorkflow1
    {
        private readonly IWorkflow2 _workflow2;

        public TestWorkflow1(IWorkflow2 workflow2)
        {
            _workflow2 = workflow2;
            Id = Guid.NewGuid().ToString();
            Name = "Workflow 2";
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public TestResponse Invoke(TestRequest request)
        {
            var log = LogManager.GetLogger("Profiler");
            log.Debug("Started workflow 1");

            try
            {
                throw new Exception("test");
            }
            catch (Exception)
            {}

            _workflow2.Invoke("test");
            log.Debug("Completed workflow 1");

            return new TestResponse
            {
                Id = 31,
                Name = "Workflow One",
                Test = new TestClass
                {
                    Id = 321,
                    Name = "Test 1",
                    Parent = new TestClass
                    {
                        Id = 432432,
                        Name = "Bob"
                    }
                }
            };
        }
    }

    public class TestWorkflow2 : IWorkflow2
    {
        private readonly IWorkflow3 _workflow3;

        public TestWorkflow2(IWorkflow3 workflow3)
        {
            _workflow3 = workflow3;
            Id = Guid.NewGuid().ToString();
            Name = "Workflow 2";
        }

        public string Id { get; set; }
        public string Name { get; set; }

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
        public string Id { get; set; }
        public string Name { get; set; }

        public TestWorkflow3()
        {
            Id = Guid.NewGuid().ToString();
            Name = "Workflow 3";
        }

        public string Invoke(string request)
        {
            var log = LogManager.GetLogger("Profiler");
            log.Debug("Started workflow 3");

            try
            {
                int zero = 0;
                if (10 / zero == 0)
                {
                    log.Debug("Devide by zero?");
                }
            }
            catch (Exception e)
            {
                log.Error("Error in workflow!", e);
            }

            log.Debug("Completed workflow 3");
            return "done";
        }
    }

    public interface IWorkflow1 : IWorkflow<TestRequest, TestResponse>
    { }

    public interface IWorkflow2 : IWorkflow<string, string>
    { }

    public interface IWorkflow3 : IWorkflow<string, string>
    { }

    public class TestRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<string> Items { get; set; }
        public IDictionary<Guid, TestClass> DictItems { get; set; }
    }

    public class TestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TestClass Test { get; set; }
    }

    public class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TestClass Parent { get; set; }
    }
}