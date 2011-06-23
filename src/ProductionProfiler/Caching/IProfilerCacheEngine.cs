
using System;

namespace ProductionProfiler.Core.Caching
{
    public interface IProfilerCacheEngine
    {
        /// <summary>
        /// Get an item from the cache, if cacheIfNotFound is true (the default) the item should be added to the cache with the 
        /// supplied key upon successful retrieval from the retrieverFunc delegate. 
        /// Optionally supply an expiry TimeSpan if this item should expire at a given point in time.
        /// </summary>
        /// <typeparam name="T">Type of the cache item</typeparam>
        /// <param name="key">key used to look up the item</param>
        /// <param name="retrieverFunc">delegate to look retrieve the item if it is not found in the cache</param>
        /// <param name="cacheIfNotFound">is true the item should be put into the cache after its retrieved from the retrieverFunc</param>
        /// <param name="expires">options expiry timespan</param>
        /// <returns></returns>
        T Get<T>(string key, Func<T> retrieverFunc, bool cacheIfNotFound = true, TimeSpan expires = default(TimeSpan)) where T : class;

        /// <summary>
        /// Remove item/items from the cache, if isKeyPrefix is true then the key itself is a prefix and we should look up
        /// all keys from the cache which start with the key supplied and remove all matching keys. This is used when paging
        /// results sets, where the first portion of the key does not change, but last sections of the key incorporate the
        /// page number and page size.
        /// </summary>
        /// <param name="key">key used to look up the item</param>
        /// <param name="isKeyPrefix">Is the supplied key just aprefix t obe used to look up all matching keys?</param>
        void Remove(string key, bool isKeyPrefix = false);
    }
}
