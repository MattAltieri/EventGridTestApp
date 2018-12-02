using EventGridTestApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EventGridTestApp.Helpers {
    public static class StorageHelper {
        
        public static bool IsImage(IFormFile file) {
            if (file.ContentType.Contains("image"))
                return true;

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig storageConfig) {

            StorageCredentials storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.ImageContainer);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            await blockBlob.UploadFromStreamAsync(fileStream);
            return await Task.FromResult(true);
        }

        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig storageConfig) {

            List<string> thumbnailUrls = new List<string>();

            StorageCredentials storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.ThumbnailContainer);
            
            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;
            
            do {
                resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, continuationToken, null, null);
                
                foreach (var blobItem in resultSegment.Results)
                    thumbnailUrls.Add(blobItem.StorageUri.PrimaryUri.ToString());

                continuationToken = resultSegment.ContinuationToken;
            } while (continuationToken != null);

            return await Task.FromResult(thumbnailUrls);
        }
    }
}