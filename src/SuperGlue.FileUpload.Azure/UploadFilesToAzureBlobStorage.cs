using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using SuperGlue.Configuration;

namespace SuperGlue.FileUpload.Azure
{
    public class UploadFilesToAzureBlobStorage : IUploadFiles
    {
        private readonly CloudBlobContainer _container;

        public UploadFilesToAzureBlobStorage(IDictionary<string, object> environment)
        {
            var settings = environment.GetSettings<AzureUploadSettings>();

            var storageAccount = CloudStorageAccount.Parse(settings.StorageAccount);

            var blobStorage = storageAccount.CreateCloudBlobClient();

            _container = blobStorage.GetContainerReference(settings.GetContainerName(environment));

            if (!_container.CreateIfNotExist()) return;

            var permissions = _container.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            _container.SetPermissions(permissions);
        }

        public Task<string> Upload(UploadFile file, IDictionary<string, object> environment)
        {
            var fileName = $"{DateTime.UtcNow.ToString("yyyy-MM-ddHHmmss")}-{file.Name}";

            var blobName = fileName.ToLower();

            var blob = _container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = file.ContentType;
            blob.UploadFromStream(file.Data);

            return Task.FromResult(fileName);
        }
    }
}