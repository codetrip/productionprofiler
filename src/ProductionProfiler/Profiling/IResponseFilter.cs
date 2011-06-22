
namespace ProductionProfiler.Core.Profiling
{
    public interface IResponseFilter : IDoNotWantToBeProxied
    {
        string Response { get; }
    }
}
