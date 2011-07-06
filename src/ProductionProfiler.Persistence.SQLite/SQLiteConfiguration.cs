
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Persistence.SQLite
{
    public class SQLiteConfiguration : IDoNotWantToBeProfiled
    {
        public string ConnectionStringName { get; set; }

        public SQLiteConfiguration(string connectionStringName)
        {
            ConnectionStringName = connectionStringName;
        }
    }
}
