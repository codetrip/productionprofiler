using System;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Persistence
{
    public interface IPersistenceProvider : IDoNotWantToBeProfiled
    {
        Type RepositoryType { get; }
        void RegisterDependentComponents(IContainer container);
    }
}
