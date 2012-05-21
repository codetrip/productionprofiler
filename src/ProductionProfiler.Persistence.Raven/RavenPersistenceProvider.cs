using System;
using System.Collections.Generic;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Document;

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
            _documentStore.DatabaseCommands.PutIndex("UrlToProfileIndex", new IndexDefinition
            {
                Map = "from doc in docs.UrlToProfiles select new { ProfilingCount = doc.ProfilingCount, Enabled = doc.Enabled, Url = doc.Url, __document_id = doc.__document_id }",
                Fields = new List<string>(new []{ "ProfilingCount" }),
                Indexes = new Dictionary<string, FieldIndexing>
                {
                    {"ProfilingCount", FieldIndexing.No}
                },
                Stores = new Dictionary<string, FieldStorage>
                {
                    {"ProfilingCount", FieldStorage.Yes}
                },
                SortOptions = new Dictionary<string, SortOptions>
                {
                    {"ProfilingCount", SortOptions.Long } 
                }
            });

            _documentStore.DatabaseCommands.PutIndex("UrlToProfileDataIndex", new IndexDefinition
            {
                Map = "from doc in docs.UrlToProfileDatas select new { Id = doc.Id, CapturedOnUtc = doc.CapturedOnUtc, Url = doc.Url, __document_id = doc.__document_id }",
                Fields = new List<string>(new[] { "CapturedOnUtc" }),
                Indexes = new Dictionary<string, FieldIndexing>
                {
                    {"CapturedOnUtc", FieldIndexing.No}
                },
                Stores = new Dictionary<string, FieldStorage>
                {
                    {"CapturedOnUtc", FieldStorage.Yes}
                },
                SortOptions = new Dictionary<string, SortOptions>
                {
                    {"CapturedOnUtc", SortOptions.String } 
                }
            });

            _documentStore.DatabaseCommands.PutIndex("ProfiledResponseIndex", new IndexDefinition
            {
                Map = "from doc in docs.ProfiledResponses select new { Id = doc.Id, __document_id = doc.__document_id, Url = doc.Url }"
            });
        }
    }
}
