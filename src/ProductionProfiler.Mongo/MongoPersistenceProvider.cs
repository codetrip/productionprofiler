using System;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Profiling.Entities;

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

        /// <summary>
        /// create two indexes for the URL field in the UrlToProfileDatabaseName and UrlToProfileDataDatabaseName databases
        /// </summary>
        public void Initialise()
        {
            using (MongoSession session = MongoSession.Connect(MongoProfilerRepository.UrlToProfileDatabaseName, _mongoConfiguration.Server, _mongoConfiguration.Port))
            {
                session.CreateIndex<UrlToProfile>("Url", true, true);
            }

            using (MongoSession session = MongoSession.Connect(MongoProfilerRepository.UrlToProfileDataDatabaseName, _mongoConfiguration.Server, _mongoConfiguration.Port))
            {
                session.CreateIndex<ProfiledRequestData>("Url", true, false);
            }
        }
    }
}
