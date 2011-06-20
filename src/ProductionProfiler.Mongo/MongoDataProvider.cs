using System;
using ProductionProfiler.Core.Interfaces;

namespace ProductionProfiler.Persistence.Mongo
{
    public class MongoDataProvider : IDataProvider
    {
        private readonly MongoConfiguration _mongoConfiguration;

        public MongoDataProvider(string mongoEndpoint, int mongoPort)
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
