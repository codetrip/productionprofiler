
namespace ProductionProfiler.Core.Caching
{
    public class NullCacheEngine : IProfilerCacheEngine
    {
        public T Get<T>(string key) where T : class
        {
            return null;
        }

        public void Put<T>(T item, string key) where T : class
        { }

        public void Remove(string key)
        { }
    }
}
