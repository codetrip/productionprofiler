using System;
using System.Collections.Generic;

namespace ProductionProfiler.Core.Profiling.Entities
{
    public class MethodInvocation
    {
        public object InvocationTarget { get; set; }
        public Type TargetType { get; set; }
        public object[] Arguments { get; set; }
        public string MethodName { get; set; }
        public object ReturnValue { get; set; }
        public Exception Exception { get; set; }
        public List<DataCollection> MethodData { get; set; }

        public MethodInvocation()
        {
            MethodData = new List<DataCollection>();
        }
    }
}