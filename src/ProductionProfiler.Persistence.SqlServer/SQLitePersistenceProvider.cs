using System;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;

namespace ProductionProfiler.Persistence.Sql
{
    public class SQLitePersistenceProvider : IPersistenceProvider
    {
        private readonly SqlConfiguration _configuration;

        public SQLitePersistenceProvider(SqlConfiguration configuration)
        {
            _configuration = configuration;
            _configuration.GenerateIds = true;
        }

        public Type RepositoryType
        {
            get { return typeof(SqlProfilerRepository); }
        }

        public void RegisterDependentComponents(IContainer container)
        {
            container.RegisterSingletonInstance(_configuration);
        }

        /// <summary>
        /// make sure the database and associated tables exist
        /// </summary>
        public void Initialise()
        {
            _configuration.SchemaName = string.Empty;
            SQLiteDataModelBuilder.BuildDataModel(_configuration);
        }
    }
}
