using ProductionProfiler.Core.Serialization;

namespace ProductionProfiler.Core.Profiling.Entities
{
    public class DataCollectionItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public DataFormat Format { get; set; }

        public DataCollectionItem()
        {
            Format = DataFormat.String;
        }

        public DataCollectionItem(string name, string value, DataFormat format = DataFormat.String) 
        {
            Name = name;
            Value = value;
            Format = format;
        }
    }
}