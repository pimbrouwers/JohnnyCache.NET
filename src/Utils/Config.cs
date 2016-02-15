using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheIO
{
    public static class Config
    {
        public static string ObjectCacheName
        {
            get
            {
                string s = System.Configuration.ConfigurationManager.AppSettings["JC-ObjectCacheName"];

                if(string.IsNullOrWhiteSpace(s))
                {
                    s = "JohnnyCache";
                }

                return s;
            }
        }

        public static string WorkingDirectory
        {
            get
            {
                string s = System.Configuration.ConfigurationManager.AppSettings["JC-WorkingDirectory"];
                if (string.IsNullOrWhiteSpace(s))
                {
                    s = @"c:\temp\JohnnyCache\";                
                }

                return s;
            }
        }

        public static int ExpirationSeconds
        {
            get
            {
                int expiration = 300;
                string s = System.Configuration.ConfigurationManager.AppSettings["JC-ExpirationInSeconds"];

                if (string.IsNullOrWhiteSpace(s))
                {
                    int.TryParse(s, out expiration);
                }

                return expiration;
            }
        }

        //////////////////////////////////
        //AZURE
        //////////////////////////////////
        public static string AzureAccountName
        {
            get
            {
                string s = System.Configuration.ConfigurationManager.AppSettings["JC-AzureAccountName"];
                if (string.IsNullOrWhiteSpace(s))
                {
                    s = null;
                }

                return s;
            }
        }

        public static string AzureAccountKey
        {
            get
            {
                string s = System.Configuration.ConfigurationManager.AppSettings["JC-AzureAccountKey"];
                if (string.IsNullOrWhiteSpace(s))
                {
                    s = null;
                }

                return s;
            }
        }

        public static string AzureDefaultBlobContainer 
        {
            get
            {
                string s = System.Configuration.ConfigurationManager.AppSettings["JC-AzureDefaultBlobContainer"];
                
                if (string.IsNullOrWhiteSpace(s))
                {
                    s = Config.ObjectCacheName;
                }

                return s;
            }
        }
    }
}
