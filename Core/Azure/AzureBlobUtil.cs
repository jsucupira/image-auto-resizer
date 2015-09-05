using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Core.Azure
{
    public class AzureBlobUtil
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureBlobUtil(string accountConnetingString)
        {
            _storageAccount = CloudStorageAccount.Parse(accountConnetingString);
        }

        public IEnumerable<string> BlobList(string containerName)
        {
            List<string> list = new List<string>();
            // Retrieve storage account from connection string.
            // Create the blob client. 
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Loop over items within the container and output the length and URI.
            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                string name = item.Uri.Segments[item.Uri.Segments.Length - 1];
                list.Add(name);
            }
            return list;
        }

        public void DownloadBlobAsFile(string containerName, string filePath, string fileName)
        {
            FileInfo file = new FileInfo(filePath + @"\" + fileName);
            if (file.Directory != null) file.Directory.Create(); // If the directory already exists, this method does nothing.
            // Create the blob client.
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob.txt"
            CloudBlob blob = container.GetBlobReference(fileName);
            blob.DownloadToFile(file.FullName, FileMode.Create);
        }
    }
}