using System;
using System.Linq;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.TypeRules;

namespace ProductionProfiler.IoC.StructureMap
{
    public class DerivedOpenGenericInterfaceConnectionScanner : IRegistrationConvention
    {
        private readonly Type _openType;

        public DerivedOpenGenericInterfaceConnectionScanner(Type openType)
        {
            _openType = openType;
            if (!_openType.IsInterface || !_openType.IsOpenGeneric())
                throw new ApplicationException(
                    "This scanning convention can only be used with open generic interface types");
        }

        public void Process(Type type, Registry registry)
        {
            if (!type.IsConcrete()) 
                return;

            var derivedTypes = type.GetInterfaces().
                                Where(i => i.GetInterfaces().
                                        Any(i2 => i2.IsGenericType &&
                                                  i2.GetGenericTypeDefinition() == _openType));

            if (derivedTypes.Count() > 0) 
                registry.For(derivedTypes.First()).Add(type);
        }
    }
}
