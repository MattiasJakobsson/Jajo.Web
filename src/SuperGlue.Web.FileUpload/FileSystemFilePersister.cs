using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.FileUpload
{
    public class FileSystemFilePersister : IPersistFiles
    {
        public Task Persist(IDictionary<string, object> environment, string name, Stream content)
        {
            var filePath = environment.ResolvePath($"~/upload/{name}");

            content.Position = 0;

            var directory = Path.GetDirectoryName(filePath);

            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var fileLocation = File.Create(filePath))
                return content.CopyToAsync(fileLocation);
        }

        public string GetUrlFor(IDictionary<string, object> environment, string file, ITransformFile transformer = null)
        {
            return $"/upload/{(transformer != null ? transformer.GetFileName(file) : file)}";
        }
    }
}