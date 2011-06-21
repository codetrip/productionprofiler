
namespace ProductionProfiler.Core.Dynamic
{
    /// <summary>
    /// Generic component which is capable of locating a specific piece of data 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface ILocator<TResult>
    {
        bool Locate(out TResult item);
    }
}
