using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CacheIO.Providers;
using System.IO;
using Newtonsoft.Json;
using CacheIO.Helpers;

namespace CacheIO.Cache
{
    
    public static class Azure
    {
        public static bool IsReady
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Config.AzureAccountName) || String.IsNullOrWhiteSpace(Config.AzureAccountKey))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Lock for multi-threaded environments
        /// </summary>
        static readonly object padlock = new object();

        /// <summary>
        /// Attempts tp write the file to the file system
        /// </summary>
        /// <param name="objToWrite"></param>
        /// <param name="fileName"></param>
        public static void AddItem(object objToWrite, string fileName)
        {
            try
            {
                string objStr = ObjectHelper.SerializeObject(objToWrite);
                byte[] bytes = StringHelper.GetBytes(objStr);

                lock(padlock)
                {
                    AzureBlob.UploadBlob(bytes, fileName);
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Attempts to read stored object from file system       
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns>Object (or null if not exists)</returns>
        public static T GetItem<T>(string fileName, int? cacheDurationSeconds = null)
        {
            try
            {
                lock(padlock)
                {
                    int expirationSeconds = (cacheDurationSeconds != null) ? (int)cacheDurationSeconds : Config.ExpirationSeconds;
                    
                    //will first check if blob is stale (expired), if not will download and turn
                    string res = AzureBlob.DownloadBlob(fileName, expirationSeconds);
                    T resp = JsonConvert.DeserializeObject<T>(res);

                    if (resp is T)
                    {
                        //we know that if we've arrive here, both mem and file cache are empty
                        //save resp in file cache (which will push into memory)
                        FileCache.AddItem(resp, fileName, cacheDurationSeconds);

                        return resp;
                    }  
                }
                              
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return default(T);
        }

        /// <summary>
        /// Attempts to delete file
        /// </summary>
        /// <param name="fileName"></param>
        public static void RemoveItem(string fileName)
        {
            try
            {
                lock(padlock)
                {
                    AzureBlob.DeleteBlob(fileName);
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
    }
}
