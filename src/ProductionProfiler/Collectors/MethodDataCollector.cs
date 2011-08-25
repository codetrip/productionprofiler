
using System.Collections.Generic;
using ProductionProfiler.Core.Profiling.Entities;
using ProductionProfiler.Core.Extensions;
using ProductionProfiler.Core.Serialization;

namespace ProductionProfiler.Core.Collectors
{
    /// <summary>
    /// MethodDataCollector collects any arguments supplied to the method as well as the return value
    /// once the method has been invoked. For each type (argument or return value) the ISerializer
    /// is used to write the public readable properties of the type and return the serialized form
    /// </summary>
    public class MethodDataCollector : IMethodDataCollector
    {
        private readonly ISerializer _serializer;

        public MethodDataCollector(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public virtual IEnumerable<DataCollectionItem> GetArguments(MethodInvocation invocation)
        {
            int count = 1;
            foreach (object argument in invocation.Arguments)
            {
                yield return new DataCollectionItem("Arg-{0}".FormatWith(count), _serializer.Serialize(argument), argument.GetType().FullName, _serializer.Format);
                count++;
            }
        }

        public virtual DataCollectionItem GetReturnValue(MethodInvocation invocation)
        {
            return new DataCollectionItem("ReturnValue", _serializer.Serialize(invocation.ReturnValue), invocation.ReturnValue.GetType().FullName, _serializer.Format);
        }
    }
}
