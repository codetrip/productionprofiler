using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace ProductionProfiler.Core.Caching
{
    public class HttpRuntimeCacheEngine : ICacheEngine
    {
        public T Get<T>(string key, Func<T> retrieverFunc, bool cacheIfNotFound = true, TimeSpan expires = default(TimeSpan)) where T : class
        {
            object item = HttpRuntime.Cache.Get(key);

            if(item == null)
            {
                item = retrieverFunc();

                if (item != null && cacheIfNotFound)
                {
                    HttpRuntime.Cache.Insert(
                        key,
                        item,
                        null,
                        expires != default(TimeSpan) ? DateTime.Now.Add(expires) : Cache.NoAbsoluteExpiration,
                        Cache.NoSlidingExpiration);
                }
            }

            return item as T;
        }

        public void Remove(string key, bool isKeyPrefix = false)
        {
            if (isKeyPrefix)
            {
                foreach (string keyFromPrefix in Keys(key))
                    HttpRuntime.Cache.Remove(keyFromPrefix);
            }
            else
                HttpRuntime.Cache.Remove(key);
        }

        private static IEnumerable<string> Keys(string keyPrefix)
        {
            IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();

            while (enumerator.MoveNext())
            {
                string cacheKey = enumerator.Key as string;

                if (cacheKey != null && cacheKey.StartsWith(keyPrefix))
                {
                    yield return cacheKey;
                }
            }
        }
    }
}
