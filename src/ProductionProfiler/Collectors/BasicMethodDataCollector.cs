
using ProductionProfiler.Core.Profiling.Entities;
using System.Linq;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Serialization;

namespace ProductionProfiler.Core.Collectors
{
    /// <summary>
    /// BasicMethodDataCollector collects any arguments supplied to the method as well as the return value
    /// once the method has been invoked. For each type (argument or return value) the SimpleTypeSerializer
    /// is used to write the public readable properties of the type to the invocation.MethodData property
    /// </summary>
    public class BasicMethodDataCollector : IMethodDataCollector
    {
        private readonly string _dataCollectionName;
        private readonly ISerializer _serializer;

        public BasicMethodDataCollector(ISerializer serializer) :
            this("Method Data", serializer)
        {}

        protected BasicMethodDataCollector(string dataCollectionName, ISerializer serializer)
        {
            _dataCollectionName = dataCollectionName;
            _serializer = serializer;
        }

        public virtual void Entry(MethodInvocation invocation)
        {
            var dataCollection = GetDataCollection(invocation);

            int count = 1;
            foreach (object argument in invocation.Arguments)
            {
                dataCollection.Data.Add(new DataCollectionItem("Argument {0}".FormatWith(count), _serializer.Serialize(argument), _serializer.Format));
                count++;
            }
        }

        public virtual void Exit(MethodInvocation invocation)
        {
            var dataCollection = GetDataCollection(invocation);
            dataCollection.Data.Add(new DataCollectionItem("ReturnValue", _serializer.Serialize(invocation.ReturnValue), _serializer.Format));
        }

        protected DataCollection GetDataCollection(MethodInvocation invocation)
        {
            var dataCollection = invocation.MethodData.FirstOrDefault(md => md.Name == _dataCollectionName);

            if (dataCollection != null)
                return dataCollection;

            dataCollection = new DataCollection(_dataCollectionName);
            invocation.MethodData.Add(dataCollection);
            return dataCollection;
        }
    }
}
