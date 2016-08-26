using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnnyCache.Cache;

namespace JohnnyCache
{
    public static class CacheIO
    {
        public static object Get<T>(string key, int? cacheDurationSeconds = null)
        {
            try
            {
                T objFromCache;

                //first check mem cache
                objFromCache = ObjectCache.GetItem<T>(key);

                if (!EqualityComparer<T>.Default.Equals(objFromCache, default(T)))
                    return objFromCache;

                //at this point we know it's not in memory
                //read from file system
                objFromCache = FileCache.GetItem<T>(key, cacheDurationSeconds);

                if(!EqualityComparer<T>.Default.Equals(objFromCache, default(T)))
                    return objFromCache;
                
                if(Azure.IsReady)
                {
                    //not in mem or file
                    //hit blob storage
                    objFromCache = Azure.GetItem<T>(key, cacheDurationSeconds);

                    if (!EqualityComparer<T>.Default.Equals(objFromCache, default(T)))
                        return objFromCache;
                }
                //TODO: else if (AmazonS3.IsReady)

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Set(object objToWrite, string key, int? cacheDurationSeconds = null)
        {
            try
            {
                //add to mem cache
                ObjectCache.AddItem(objToWrite, key, cacheDurationSeconds);

                //write to the file system
                //(also writes to object cache)
                FileCache.AddItem(objToWrite, key, cacheDurationSeconds);

                //Is Azure Setup?
                if (Azure.IsReady)
                {
                    //writes to azure blob storage
                    //(also writes to file cache)
                    //(also writes to object cache)
                    Azure.AddItem(objToWrite, key);
                }
                //TODO: else if (AmazonS3.IsReady)
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Delete(string key)
        {
            try
            {
                //clear out of mem cache
                ObjectCache.RemoveItem(key);

                //delete file
                FileCache.RemoveItem(key);

                //delete azure blob
                Azure.RemoveItem(key);

                //TODO: AmazonS3.RemoveItem(key);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
