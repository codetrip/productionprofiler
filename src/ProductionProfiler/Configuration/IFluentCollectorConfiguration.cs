using System;
using System.Collections.Generic;

namespace ProductionProfiler.Core.Configuration
{
    public interface IFluentCollectorConfiguration
    {
        IFluentConfiguration ForTypesAssignableFrom(IEnumerable<Type> types);
        IFluentConfiguration ForAnyType();
        IFluentConfiguration ForAnyUnmappedType();
    }
}