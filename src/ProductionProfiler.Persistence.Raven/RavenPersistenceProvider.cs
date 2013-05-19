using System;
using System.ComponentModel.Composition.Hosting;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Persistence.Raven
{
    public class RavenPersistenceProvider : IPersistenceProvider
    {
        private IDocumentStore _documentStore;
        private readonly RavenConfiguration _ravenConfiguration;

        public RavenPersistenceProvider(string ravenEndpoint, string databaseName = null)
        {
            _ravenConfiguration = new RavenConfiguration(ravenEndpoint, databaseName);
        }

        public Type RepositoryType
        {
            get { return typeof(RavenProfilerRepository); }
        }

        public void RegisterDependentComponents(IContainer container)
        {
            _documentStore = new DocumentStore
            {
                Url = _ravenConfiguration.RavenEndpoint,
                EnlistInDistributedTransactions = false,
            };
            _documentStore.Initialize();
            container.RegisterSingletonInstance(_documentStore);
            container.RegisterSingletonInstance(_ravenConfiguration);
        }

        /// <summary>
        /// create two indexes for the URL field in the UrlToProfileDatabaseName and UrlToProfileDataDatabaseName databases
        /// </summary>
        public void Initialise()
        {
            if (_ravenConfiguration.DatabaseName.IsNotNullOrEmpty())
            {
                IndexCreation.CreateIndexes(
                    new CompositionContainer(new AssemblyCatalog(GetType().Assembly), new ExportProvider[0]), 
                    _documentStore.DatabaseCommands.ForDatabase(_ravenConfiguration.DatabaseName), 
                    _documentStore.Conventions);
            }   
            else
            {
                IndexCreation.CreateIndexes(GetType().Assembly, _documentStore);   
            }            
        }
    }
}
