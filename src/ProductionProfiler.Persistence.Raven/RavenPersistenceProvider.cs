using System;
using System.Collections.Generic;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace ProductionProfiler.Persistence.Raven
{
    public class RavenPersistenceProvider : IPersistenceProvider
    {
        private IDocumentStore _documentStore;
        private readonly RavenConfiguration _ravenConfiguration;

        public RavenPersistenceProvider(string ravenEndpoint)
        {
            _ravenConfiguration = new RavenConfiguration(ravenEndpoint);
        }

        public Type RepositoryType
        {
            get { return typeof(RavenProfilerRepository); }
        }

        public void RegisterDependentComponents(IContainer container)
        {
            _documentStore = new DocumentStore { Url = _ravenConfiguration.RavenEndpoint };
            _documentStore.Initialize();
            container.RegisterSingletonInstance(_documentStore);
            container.RegisterSingletonInstance(_ravenConfiguration);
        }

        /// <summary>
        /// create two indexes for the URL field in the UrlToProfileDatabaseName and UrlToProfileDataDatabaseName databases
        /// </summary>
        public void Initialise()
        {
            IndexCreation.CreateIndexes(GetType().Assembly, _documentStore);
        }
    }
}
