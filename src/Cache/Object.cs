using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JohnnyCache.Cache
{
    public static class ObjectCache
    {
        //invoke object cache
        private static MemoryCache _cache;
        private static MemoryCache Cache
        {
            get
            {
                if (_cache == null)
                {
                    using (ExecutionContext.SuppressFlow())
                    {
                        // Create memory cache instance under disabled execution context flow
                        _cache = new MemoryCache(Config.ObjectCacheName);
                    }
                }

                return _cache;
            }
        }

        /// <summary>
        /// Lock for multi-threaded environments
        /// </summary>
        static readonly object padlock = new object();

        /// <summary>
        /// Add Item to Object Cache
        /// </summary>
        /// <param name="objToWrite"></param>
        /// <param name="key"></param>
        public static void AddItem(object objToWrite, string key, int? cacheDurationSeconds = null)
        {            
            lock (padlock)
            {
                //do we have the object in object cache already?
                if (ObjectCache.Cache.Contains(new KeyValuePair<string, object>(key, objToWrite)))
                    ObjectCache.RemoveItem(key);

                //set new object into memory
                Cache.Set(key, objToWrite, new CacheItemPolicy() {AbsoluteExpiration = DateTime.Now.AddSeconds(Config.ExpirationSeconds), Priority = CacheItemPriority.NotRemovable});
            }
        }

        /// <summary>
        /// Remove Item from Object Cache
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveItem(string key)
        {
            lock (padlock)
            {
                try
                {
                    Cache.Remove(key);
                }
                catch { } //try to remove, or die if not exists                
            }
        }

        /// <summary>
        /// Retrieve Item from Object Cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetItem<T>(string key)
        {
            lock (padlock)
            {
                var res = Cache.Get(key);

                if (res is T)
                    return (T)res;
                else
                    return default(T);
            }
        }

    } 
}
