
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ProductionProfiler.Core.Profiling.Entities
{
    public class DataCollection
    {
        public string Name { get; set; }
        public List<DataCollectionItem> Data { get; set; }

        public DataCollection()
        {
            Data = new List<DataCollectionItem>();
        }

        public DataCollection(string name) : this()
        {
            Name = name;
        }

        public DataCollection(string name, NameValueCollection data) : this(name)
        {
            Data = data.AllKeys.Select(k => new DataCollectionItem(k, data.Get(k))).ToList();
        }
    }
}
