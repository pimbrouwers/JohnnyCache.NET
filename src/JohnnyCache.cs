using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheIO.Cache;

namespace CacheIO
{
    public static class JohnnyCache
    {
        public static object Get<T>(string key)
        {
            try
            {                
                //first check mem cache
                T objFromCache = (T)ObjectCache.GetItem<T>(key);
                if (objFromCache != null)
                    return objFromCache;

                //at this point we know it's not in memory
                //read from file system
                return FileCache.GetItem<T>(key);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Set(object objToWrite, string key)
        {
            try
            {
                //clear out mem cache
                ObjectCache.RemoveItem(key);

                //write to the file system
                //(also writes to object cache)
                FileCache.AddItem(objToWrite, key);
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
