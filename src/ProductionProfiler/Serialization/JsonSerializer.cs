
using System;
using Newtonsoft.Json;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Core.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception e)
            {
                return "{\"Error\":\"" + "Failed to serialize type:={0}, Message:={1}".FormatWith(obj.GetType(), e.Message) + "\"}";
            }
        }

        public DataFormat Format
        {
            get { return DataFormat.Json; }
        }
    }
}