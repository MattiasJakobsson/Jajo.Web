using System;
using System.Collections.Generic;

namespace SuperGlue.FileUpload.Azure
{
    public class AzureUploadSettings
    {
        private Func<IDictionary<string, object>, string> _getContainerName;

        public string StorageAccount { get; private set; }

        public string GetContainerName(IDictionary<string, object> environment)
        {
            return _getContainerName(environment);
        }

        public AzureUploadSettings UseStorageAccount(string account)
        {
            StorageAccount = account;

            return this;
        }

        public AzureUploadSettings GetContainerNameUsing(Func<IDictionary<string, object>, string> getContainerName)
        {
            _getContainerName = getContainerName;

            return this;
        }
    }
}