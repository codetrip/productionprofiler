
namespace ProductionProfiler.Web.Models
{
    public interface IWorkflow<in TRequest, out TResponse> : IWorkflow
    {
        TResponse Invoke(TRequest request);
    }
    
    public interface IWorkflow
    {}
}