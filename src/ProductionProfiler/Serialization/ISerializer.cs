
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Serialization
{
    public interface ISerializer : IDoNotWantToBeProxied
    {
        string Serialize(object obj);
        DataFormat Format { get; }
    }

    public enum DataFormat
    {
        Xml,
        Json,
        String
    }
}
