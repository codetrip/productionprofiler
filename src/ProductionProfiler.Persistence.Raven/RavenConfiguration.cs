
namespace ProductionProfiler.Persistence.Raven
{
    public class RavenConfiguration
    {
        public string RavenEndpoint { get; set; }
        public string DatabaseName { get; set; }

        public RavenConfiguration(string ravenEndpoint, string databaseName)
        {
            RavenEndpoint = ravenEndpoint;
            DatabaseName = databaseName;
        }
    }
}
