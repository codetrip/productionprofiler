
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Persistence.Sql
{
    public class SqlConfiguration : IDoNotWantToBeProfiled
    {
        public string ConnectionStringName { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public string OutputScriptPath { get; set; }
        public bool GenerateIds { get; set; }

        public SqlConfiguration(string connectionStringName, string databaseName)
            : this(connectionStringName, databaseName, "dbo")
        {}

        public SqlConfiguration(string connectionStringName, string databaseName, string schemaName)
            : this(connectionStringName, databaseName, schemaName, null)
        {}

        public SqlConfiguration(string connectionStringName, string databaseName, string schemaName, string outputScriptPath)
        {
            ConnectionStringName = connectionStringName;
            DatabaseName = databaseName;
            SchemaName = schemaName;
            OutputScriptPath = outputScriptPath;
        }
    }
}
