using System;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;

namespace ProductionProfiler.Persistence.SQLite
{
    public class SQLitePersistenceProvider : IPersistenceProvider
    {
        private readonly SQLiteConfiguration _configuration;

        public SQLitePersistenceProvider(SQLiteConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Type RepositoryType
        {
            get { return typeof(SQLiteProfilerRepository); }
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
            SQLiteDataModelBuilder.BuildDataModel(_configuration);
        }
    }
}
