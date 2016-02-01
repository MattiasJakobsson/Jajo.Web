using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.FileSystem;

namespace SuperGlue.FileUpload
{
    public class DefaultFileUploader : IUploadFiles
    {
        private readonly IFileSystem _fileSystem;

        public DefaultFileUploader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task<string> Upload(UploadFile file, IDictionary<string, object> environment)
        {
            var settings = environment.GetSettings<FileUploadSettings>();

            var path = environment.ResolvePath($"~/{settings.UploadPath}");

            var filePath = $"{path}/{file.Name}";

            file.Data.Position = 0;

            await _fileSystem.CreateDirectory(path).ConfigureAwait(false);

            await _fileSystem.WriteStreamToFile(filePath, file.Data).ConfigureAwait(false);

            return file.Name;
        }
    }
}