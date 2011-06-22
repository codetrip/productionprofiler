using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Serialization;
using ProductionProfiler.Web.Models;

namespace ProductionProfiler.Web.Profilng
{
    public class WorkflowMethodDataCollector : BasicMethodDataCollector
    {
        public WorkflowMethodDataCollector(ISerializer serializer) :
            base("Workflow", serializer)
        { }

        public override void Entry(MethodInvocation invocation)
        {
            base.Entry(invocation);

            IWorkflow wf = invocation.InvocationTarget as IWorkflow;

            if(wf != null)
            {
                var collection = GetDataCollection(invocation);
                collection.Data.Add(new DataCollectionItem("Id", wf.Id));
                collection.Data.Add(new DataCollectionItem("Name", wf.Name));
            }
        }
    }
}