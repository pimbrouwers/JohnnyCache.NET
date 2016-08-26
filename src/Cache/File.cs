using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;
using JohnnyCache.Helpers;

namespace JohnnyCache.Cache
{
    public static class FileCache
    {
        /// <summary>
        /// Lock for multi-threaded environments
        /// </summary>
        static readonly object padlock = new object();

        /// <summary>
        /// Attempts tp write the file to the file system
        /// </summary>
        /// <param name="objToWrite"></param>
        /// <param name="fileName"></param>
        public static void AddItem(object objToWrite, string fileName, int? cacheDurationSeconds = null)
        {
            try
            {
                string objStr = ObjectHelper.SerializeObject(objToWrite);
                string path = ResolveFilePath(fileName);

                if (!String.IsNullOrWhiteSpace(path))
                {
                    lock (padlock)
                    {
                        if (File.Exists(path))
                            FileCache.RemoveItem(fileName);

                        File.WriteAllText(path, objStr);

                        //write to object cache
                        ObjectCache.AddItem(objToWrite, fileName, cacheDurationSeconds);
                    }
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
            T resp = default(T);

            string path = ResolveFilePath(fileName);

            if (!String.IsNullOrWhiteSpace(path))
            {
                FileInfo fi = new FileInfo(path);

                if (fi.Exists)
                {
                    try
                    {
                        lock (padlock)
                        {
                            int expirationSeconds = (cacheDurationSeconds != null) ? (int)cacheDurationSeconds : Config.ExpirationSeconds;

                            if (DateTime.UtcNow - fi.LastWriteTimeUtc > TimeSpan.FromSeconds(expirationSeconds))
                            {
                                fi.Delete();
                            }
                            else
                            {
                                string res = File.ReadAllText(path);

                                resp = JsonConvert.DeserializeObject<T>(res);

                                //check if the objet is in mem cache, if not add it
                                if (ObjectCache.GetItem<T>(fileName) == null)
                                    ObjectCache.AddItem(resp, fileName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        fi.Delete(); //something is probably wrong with the data in the file cache, so delete
                        throw ex;
                    }

                }
            }

            return resp;
        }

        /// <summary>
        /// Attempts to delete file
        /// </summary>
        /// <param name="fileName"></param>
        public static void RemoveItem(string fileName)
        {
            try
            {
                string path = ResolveFilePath(fileName);

                if (!String.IsNullOrWhiteSpace(path))
                {
                    lock (padlock)
                    {
                        File.Delete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string WorkingDirectory
        {
            get
            {
                string wd = Config.WorkingDirectory;

                //check if working directory exists
                if (!Directory.Exists(wd))
                {
                    try
                    {
                        //try to create directory
                        lock (padlock)
                        {
                            Directory.CreateDirectory(wd);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                return wd;
            }
        }

        private static string ResolveFilePath(string fileName)
        {
            string wd = FileCache.WorkingDirectory;

            //check if there's a trailing slash, if not add
            if (wd.Substring(wd.Length - 1) != "/")
            {
                fileName = String.Format("/{0}", fileName);
            }

            return String.Format("{0}{1}", wd, fileName);
        }
    }
}
