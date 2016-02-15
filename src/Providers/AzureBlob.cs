using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading;
using System.IO;

namespace CacheIO.Providers
{
    public static class AzureBlob
    {

        /// <summary>
        /// Reference to the predefined storage credentials
        /// </summary>
        private static StorageCredentials _credentials;
        internal static StorageCredentials Credentials
        {
            get
            {
                if (_credentials == null)
                    _credentials = new StorageCredentials(Config.AzureAccountName, Config.AzureAccountKey);

                return _credentials;
            }
        }

        /// <summary>
        /// reference to the default blob container name (from config)
        /// </summary>
        private static string _blobContainerName;
        internal static string BlobContainerName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_blobContainerName))
                    _blobContainerName = Config.AzureDefaultBlobContainer;

                return _blobContainerName;
            }
        }

        /// <summary>
        /// reference to the default blob container
        /// </summary>
        private static CloudBlobContainer _blobContainer;
        internal static CloudBlobContainer BlobContainer
        {
            get
            {
                if (_blobContainer == null)
                {
                    int attempt = 0;
                    int MaxAttempts = 3;

                    while (++attempt <= MaxAttempts)
                    {
                        try
                        {
                            StorageCredentials cred = Credentials;
                            CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                            _blobContainer = blobClient.GetContainerReference(AzureBlob.BlobContainerName);
                            _blobContainer.CreateIfNotExists();
                            break;
                        }
                        catch (Microsoft.WindowsAzure.Storage.StorageException ex)
                        {
                            //do nothing
                        }
                        catch (Exception ex)
                        {
                            if (attempt == MaxAttempts)
                            {
                                throw ex;
                            }

                            Thread.Sleep(100);
                        }
                    }
                }

                return _blobContainer;
            }

        }



        /// <summary>
        /// Create/Updated Block Blob within specificed Container
        /// </summary>
        /// <param name="tmpFilePath"></param>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        internal static void UploadBlob(byte[] bytes, string fileName)
        {

            CloudBlockBlob blockBlob = AzureBlob.BlobContainer.GetBlockBlobReference(fileName);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                int attempt = 0;
                int MaxAttempts = 3;

                while (++attempt <= MaxAttempts)
                {
                    try
                    {
                        blockBlob.UploadFromStream(stream);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (attempt == MaxAttempts)
                        {
                            throw ex;
                        }

                        Thread.Sleep(100);
                    }
                }

            }

        }

        /// <summary>
        /// Download Block Blob within specificed Container
        /// </summary>
        /// <param name="tmpFilePath"></param>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        internal static string DownloadBlob(string fileName)
        {

            CloudBlockBlob blockBlob = AzureBlob.BlobContainer.GetBlockBlobReference(fileName);

            if (blockBlob.Exists())
            {

                //check if blob is stale
                if (AzureBlob.IsStale(blockBlob))
                {
                    AzureBlob.DeleteBlob(blockBlob);
                }
                else
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        int attempt = 0;
                        int MaxAttempts = 3;

                        while (++attempt <= MaxAttempts)
                        {
                            try
                            {
                                blockBlob.DownloadToStream(stream);
                                break;
                            }
                            catch (Exception ex)
                            {
                                if (attempt == MaxAttempts)
                                {
                                    throw ex;
                                }

                                Thread.Sleep(100);
                            }
                        }

                        string res = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                    }
                }


            }

            return null;
        }

        /// <summary>
        /// Delete a file from blob
        /// exits if file not found
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        internal static void DeleteBlob(string fileName)
        {
            CloudBlockBlob blockBlob = AzureBlob.BlobContainer.GetBlockBlobReference(fileName);

            if (blockBlob.Exists())
            {
                AzureBlob.DeleteBlob(blockBlob);
            }
        }

        private static void DeleteBlob(CloudBlockBlob blockBlob)
        {
            int attempt = 0;
            int MaxAttempts = 3;

            while (++attempt <= MaxAttempts)
            {
                try
                {
                    blockBlob.DeleteIfExists();
                    break;
                }
                catch (Exception ex)
                {
                    if (attempt == MaxAttempts)
                    {
                        throw ex;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        private static bool IsStale(CloudBlockBlob blockBlob)
        {
            DateTimeOffset? now = DateTimeOffset.Now;
            DateTimeOffset? blobDate = blockBlob.Properties.LastModified;

            if (now.HasValue && blobDate.HasValue)
            {
                DateTimeOffset _now = (DateTimeOffset)now;
                DateTimeOffset _blobDate = (DateTimeOffset)blobDate;

                if (_blobDate.Subtract(_now) > new TimeSpan(0, 0, Config.ExpirationSeconds))
                {
                    return true;
                }

            }

            return false;
        }


    }
}
