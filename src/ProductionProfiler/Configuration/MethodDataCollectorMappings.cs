using System;
using System.Collections.Generic;
using System.Linq;
using ProductionProfiler.Core.Collectors;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Configuration
{
    public class MethodDataCollectorMappings
    {
        private readonly object _syncLock = new object();
        private readonly IDictionary<Type, IEnumerable<string>> _mappedCollectors = new Dictionary<Type, IEnumerable<string>>();
        private readonly IList<Type> _mappedTypes = new List<Type>();
        private readonly IList<CollectorMapping> _collectorMappings = new List<CollectorMapping>();
        private readonly IDictionary<Type, bool> _cachedInputOutputDataCollectorTypes = new Dictionary<Type, bool>();

        public IList<Type> InputOutputMethodDataTypes { get; set; }

        public IEnumerable<IMethodDataCollector> GetMethodDataCollectorsForType(Type methodTargetType)
        {
            if(!_mappedCollectors.ContainsKey(methodTargetType))
            {
                lock(_syncLock)
                {
                    if(!_mappedCollectors.ContainsKey(methodTargetType))
                    {
                        _mappedCollectors.Add(methodTargetType, MapTargetType(methodTargetType).ToList());
                    }
                }
            }

            var keys = _mappedCollectors[methodTargetType];

            if(keys != null)
            {
                foreach(string key in keys)
                {
                    yield return ProfilerContext.Container.Resolve<IMethodDataCollector>(key);
                }
            }
        }

        public bool ShouldCollectInputOutputDataForType(Type methodTargetType)
        {
            if (InputOutputMethodDataTypes == null || InputOutputMethodDataTypes.Count == 0)
                return false;

            if (!_cachedInputOutputDataCollectorTypes.ContainsKey(methodTargetType))
            {
                lock (_syncLock)
                {
                    if (!_cachedInputOutputDataCollectorTypes.ContainsKey(methodTargetType))
                    {
                        _cachedInputOutputDataCollectorTypes.Add(methodTargetType, InputOutputMethodDataTypes.Where(t => t.IsAssignableFrom(methodTargetType)).Any());
                    }
                }
            }

            return _cachedInputOutputDataCollectorTypes[methodTargetType];
        }

        public void AddMapping(CollectorMapping mapping)
        {
            _collectorMappings.Add(mapping);

            if (mapping.ForTypesAssignableFrom != null)
            {
                foreach (var type in mapping.ForTypesAssignableFrom)
                {
                    if (!_mappedTypes.Contains(type))
                        _mappedTypes.Add(type);
                }
            }
        }

        public bool IsCollectorTypeMapped(Type collectorType)
        {
            return _collectorMappings.Any(m => m.CollectorType == collectorType);
        }

        public bool AnyMappedTypes()
        {
            return _collectorMappings.Count > 0;
        }

        private IEnumerable<string> MapTargetType(Type targetType)
        {
            //yield any IMethodDataCollectors configured for all types
            foreach (var mapping in _collectorMappings.Where(m => m.ForAnyType).Select(m => m.CollectorType.FullName))
                yield return mapping;

            //the targetType is considered mapped if it is assignable from any of the _mappedTypes
            bool mapped = _mappedTypes.Any(t => t.IsAssignableFrom(targetType));

            if (!mapped)
            {
                //yield any IMethodDataCollectors configured for any types that are not mapped, if the targetType is not mapped
                foreach (var mapping in _collectorMappings.Where(m => m.ForAnyUnmappedType).Select(m => m.CollectorType.FullName))
                    yield return mapping;
            }

            //finally yield any IMethodDataCollectors where the mapping has a type which the targetType is assignable from
            foreach (var mapping in _collectorMappings.Where(m => m.ForTypesAssignableFrom != null)
                                                      .Where(m => m.ForTypesAssignableFrom.Any(t => t.IsAssignableFrom(targetType)))
                                                      .Select(m => m.CollectorType.FullName))
                yield return mapping;
        }
    }
}