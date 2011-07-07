
namespace ProductionProfiler.Tests.Components
{
    public interface IWorkflow<in TRequest, out TResponse> : IWorkflow
    {
        TResponse Invoke(TRequest request);
    }
    
    public interface IWorkflow
    {
        string Id { get; set; }
        string Name { get; set; }
    }
}