using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;

namespace CacheIO.Cache
{
    public static class FileCache
    {
        private static string WorkingDirectory {
            get
            {
                string wd = Config.WorkingDirectory;

                //check if working directory exists
                if (!Directory.Exists(wd))
                {
                    try
                    {
                        //try to create directory
                        Directory.CreateDirectory(wd);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                return wd;
            }
        }

        /// <summary>
        /// Attempts tp write the file to the file system
        /// </summary>
        /// <param name="objToWrite"></param>
        /// <param name="fileName"></param>
        public static void AddItem (object objToWrite, string fileName)
        {
            try
            {
                Type type = objToWrite.GetType();
                string objStr = null;

                if (type == typeof(DataTable))
                    objStr = JsonConvert.SerializeObject(objToWrite, new DataTableConverter());
                else if (type == typeof(DataSet))
                    objStr = JsonConvert.SerializeObject(objToWrite, new DataSetConverter());
                else
                    objStr = JsonConvert.SerializeObject(objToWrite);

                string path = ResolveFilePath(fileName);

                if (!String.IsNullOrWhiteSpace(path))
                {
                    if (File.Exists(path))
                        FileCache.RemoveItem(fileName);

                    File.WriteAllText(path, objStr);

                    //write to object cache
                    ObjectCache.AddItem(objToWrite, fileName);
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
        public static object GetItem<T>(string fileName)
        {
            try
            {            
                string path = ResolveFilePath(fileName);

                if (!String.IsNullOrWhiteSpace(path))
                {
                    FileInfo fi = new FileInfo(path);
                    
                    if(fi.Exists)
                    {
                        DateTime now = DateTime.Now;

                        if (fi.CreationTime.Subtract(now) > new TimeSpan(0, 0, Config.ExpirationSeconds))
                        {
                            fi.Delete();
                        }
                        else
                        {
                            string res = File.ReadAllText(path);

                            T resp = JsonConvert.DeserializeObject<T>(res);

                            //check if the objet is in mem cache, if not add it
                            if (ObjectCache.GetItem<T>(fileName) == null)
                                ObjectCache.AddItem(resp, fileName);

                            return resp;
                        }
                    }
                    
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

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
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private static string ResolveFilePath (string fileName)
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
