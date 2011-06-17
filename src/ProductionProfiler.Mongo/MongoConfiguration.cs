
namespace ProductionProfiler.Mongo
{
    public class MongoConfiguration
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string ConnectionOptions { get; set; }

        public MongoConfiguration(string server, string port)
        {
            Server = server;
            Port = port;
        }
    }
}
