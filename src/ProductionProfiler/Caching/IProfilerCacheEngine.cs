
namespace ProductionProfiler.Core.Caching
{
    public interface IProfilerCacheEngine
    {
        /// <summary>
        /// Retrieve an item from the cache
        /// </summary>
        /// <typeparam name="T">Type of the cache item</typeparam>
        /// <param name="key">key used to look up the item</param>
        /// <returns></returns>
        T Get<T>(string key) where T : class;

        /// <summary>
        /// Put the supplied item in the cache using the key suppplied
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="key"></param>
        void Put<T>(T item, string key) where T : class;

        /// <summary>
        /// Remove item from the cache for the specified key
        /// </summary>
        /// <param name="key">key used to look up the item</param>
        void Remove(string key);
    }
}
