using System;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Persistence
{
    public interface IPersistenceProvider : IDoNotWantToBeProfiled
    {
        /// <summary>
        /// The Type that implements IProfilerRepository 
        /// </summary>
        Type RepositoryType { get; }
        /// <summary>
        /// called on app start up and is an opportunity for the persistence provider
        /// to register any dependent components, i.e. configuration for the provider
        /// </summary>
        /// <param name="container"></param>
        void RegisterDependentComponents(IContainer container);
        /// <summary>
        /// Initialise is called on app start up and provides an opportunity for the
        /// persistence provider to perform any set up required.
        /// </summary>
        void Initialise();
    }
}
