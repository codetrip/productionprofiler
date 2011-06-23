
namespace ProductionProfiler.Core.Profiling
{
    public interface IResponseFilter : IDoNotWantToBeProfiled
    {
        string Response { get; }
    }
}
