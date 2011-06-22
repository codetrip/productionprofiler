using System;
using System.Collections.Generic;

namespace ProductionProfiler.Core.Configuration
{
    public class CollectorMapping
    {
        public Type CollectorType { get; set; }
        public IEnumerable<Type> ForTypesAssignableFrom { get; set; }
        public bool ForAnyUnmappedType { get; set; }
        public bool ForAnyType { get; set; }
    }
}