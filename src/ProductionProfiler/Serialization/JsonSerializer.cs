
using Newtonsoft.Json;

namespace ProductionProfiler.Core.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public DataFormat Format
        {
            get { return DataFormat.Json; }
        }
    }
}