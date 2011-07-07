using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Profiling.Entities;

namespace ProductionProfiler.Tests.Components
{
    public class WorkflowMethodDataCollector : IMethodDataCollector
    {
        public void Entry(MethodInvocation invocation)
        {
            IWorkflow wf = invocation.InvocationTarget as IWorkflow;

            if(wf != null)
            {
                var collection = new DataCollection("Workflow");
                collection.Data.Add(new DataCollectionItem("Id", wf.Id));
                collection.Data.Add(new DataCollectionItem("Name", wf.Name));
                invocation.MethodData.Add(collection);
            }
        }

        public void Exit(MethodInvocation invocation)
        {}
    }
}