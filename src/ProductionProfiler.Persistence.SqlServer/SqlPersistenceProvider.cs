using System;
using System.Text.RegularExpressions;
using ProductionProfiler.Core.Configuration;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Persistence;
using ProductionProfiler.Core.Extensions;

namespace ProductionProfiler.Persistence.Sql
{
    public class SqlPersistenceProvider : IPersistenceProvider
    {
        private readonly SqlConfiguration _configuration;

        public SqlPersistenceProvider(SqlConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Type RepositoryType
        {
            get { return typeof(SqlProfilerRepository); }
        }

        public void RegisterDependentComponents(IContainer container)
        {
            if (_configuration.SchemaName.IsNullOrEmpty())
                _configuration.SchemaName = "dbo";

            container.RegisterSingletonInstance(_configuration);
        }

        /// <summary>
        /// make sure the database and associated tables exist
        /// </summary>
        public void Initialise()
        {
            //first validate the schema name is A-Z a-z 0-9 characters only, this is injected into queries 
            //so need to ensure it cant cause a sql injection security risk, however unlikely
            if (!Regex.IsMatch(_configuration.SchemaName, @"^[a-zA-Z0-9]+$"))
            {
                throw new ProfilerConfigurationException("SchemaName for SqlPersistenceProvider must contain only a-z, A-Z and 0-9 characters.");
            }

            //ensure the database has all required objects
            SqlDataModelBuilder.BuildDataModel(_configuration);
        }
    }
}
