using System;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;

namespace ProductionProfiler.Persistence.Mongo
{
    public class MongoPersistenceProvider : IPersistenceProvider
    {
        private readonly MongoConfiguration _mongoConfiguration;

        public MongoPersistenceProvider(string mongoEndpoint, int mongoPort)
        {
            _mongoConfiguration = new MongoConfiguration(mongoEndpoint, mongoPort.ToString());
        }

        public Type RepositoryType
        {
            get { return typeof(MongoProfilerRepository); }
        }

        public void RegisterDependentComponents(IContainer container)
        {
            container.RegisterSingletonInstance(_mongoConfiguration);
        }
    }
}
