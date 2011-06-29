using System;
using Norm.Protocol.Messages;
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
        /// create two indexes for the URL field in the ProfiledRequestDatabaseName and ProfiledRequestDataDatabaseName databases
        /// </summary>
        public void Initialise()
        {
            using (MongoSession session = MongoSession.Connect(MongoProfilerRepository.ProfiledRequestDatabaseName, _mongoConfiguration.Server, _mongoConfiguration.Port))
            {
                session.CreateIndex<ProfiledRequest, string>(u => u.Url, "profiledrequest_url", true, IndexOption.Ascending);
            }

            using (MongoSession session = MongoSession.Connect(MongoProfilerRepository.ProfiledRequestDataDatabaseName, _mongoConfiguration.Server, _mongoConfiguration.Port))
            {
                session.CreateIndex<ProfiledRequestData, string>(u => u.Url, "profiledrequestdata_url", false, IndexOption.Ascending);
            }
        }
    }
}
