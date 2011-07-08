
namespace ProductionProfiler.Persistence.Raven
{
    public class RavenConfiguration
    {
        public string RavenEndpoint { get; set; }

        public RavenConfiguration(string ravenEndpoint)
        {
            RavenEndpoint = ravenEndpoint;
        }
    }
}
