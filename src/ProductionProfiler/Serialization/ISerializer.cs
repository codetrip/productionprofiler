
using System;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Serialization
{
    public interface ISerializer : IDoNotWantToBeProfiled
    {
        string Serialize(object obj);
        DataFormat Format { get; }
    }

    [Serializable]
    public enum DataFormat
    {
        Xml,
        Json,
        String
    }
}
