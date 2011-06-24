using System.Web;
using System.Web.Caching;

namespace ProductionProfiler.Core.Caching
{
    public class HttpRuntimeCacheEngine : IProfilerCacheEngine
    {
        public T Get<T>(string key) where T : class
        {
            return HttpRuntime.Cache.Get(key) as T;
        }

        public void Put<T>(T item, string key) where T : class
        {
            HttpRuntime.Cache.Insert(
                key,
                item,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration);
        }

        public void Remove(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }
    }
}
