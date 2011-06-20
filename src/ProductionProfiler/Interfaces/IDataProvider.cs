
using System;

namespace ProductionProfiler.Core.Interfaces
{
    public interface IDataProvider
    {
        Type RepositoryType { get; }
        void RegisterDependentComponents(IContainer container);
    }
}
