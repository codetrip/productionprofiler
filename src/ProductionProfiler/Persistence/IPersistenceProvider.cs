using System;
using ProductionProfiler.Core.IoC;

namespace ProductionProfiler.Core.Persistence
{
    public interface IPersistenceProvider
    {
        Type RepositoryType { get; }
        void RegisterDependentComponents(IContainer container);
    }
}
