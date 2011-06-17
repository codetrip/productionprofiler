
using System;

namespace ProductionProfiler.Interfaces
{
    public interface IDataProvider
    {
        Type RepositoryType { get; }
        void RegisterDependentComponents(IContainer container);
    }
}
