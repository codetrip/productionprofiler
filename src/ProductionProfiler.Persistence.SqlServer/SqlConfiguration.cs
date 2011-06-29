
namespace ProductionProfiler.Persistence.SqlServer
{
    public class SqlConfiguration
    {
        public string ConnectionStringName { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }

        public SqlConfiguration(string connectionStringName, string databaseName)
            : this(connectionStringName, databaseName, "dbo")
        {}

        public SqlConfiguration(string connectionStringName, string databaseName, string schemaName)
        {
            ConnectionStringName = connectionStringName;
            DatabaseName = databaseName;
            SchemaName = schemaName;
        }
    }
}
