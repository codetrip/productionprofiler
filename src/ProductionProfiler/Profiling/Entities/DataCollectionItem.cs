using System;
using ProductionProfiler.Core.Serialization;

namespace ProductionProfiler.Core.Profiling.Entities
{
    [Serializable]
    public class DataCollectionItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public DataFormat Format { get; set; }

        public DataCollectionItem()
        {
            Format = DataFormat.String;
        }

        public DataCollectionItem(string name, string value, string type = null, DataFormat format = DataFormat.String) 
        {
            Name = name;
            Value = value;
            Format = format;
            Type = type;
        }
    }
}