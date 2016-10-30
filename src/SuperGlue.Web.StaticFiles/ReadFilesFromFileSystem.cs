using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.FileSystem;

namespace SuperGlue.Web.StaticFiles
{
    public class ReadFilesFromFileSystem : IReadFiles
    {
        private readonly IFileSystem _fileSystem;
        private readonly IDictionary<string, object> _environment;
        private readonly IEnumerable<string> _defaultFiles;
        private readonly string _basePath;

        public ReadFilesFromFileSystem(IFileSystem fileSystem, IDictionary<string, object> environment, IEnumerable<string> defaultFiles, string basePath = "")
        {
            _fileSystem = fileSystem;
            _environment = environment;
            _defaultFiles = defaultFiles;
            _basePath = basePath;
        }

        public ReadResult TryRead(string path)
        {
            var currentPath = path;

            if(Exists(currentPath))
                return new ReadResult(true, () => Read(currentPath), currentPath);

            foreach (var defaultFile in _defaultFiles)
            {
                currentPath = Path.Combine(path, defaultFile);

                if(Exists(currentPath))
                    return new ReadResult(true, () => Read(currentPath), currentPath);
            }

            return new ReadResult(false, () => Task.FromResult<Stream>(new MemoryStream()), path);
        }

        private bool Exists(string path)
        {
            var filePath = _environment.ResolvePath($"~/{_basePath}/{path}");

            return _fileSystem.FileExists(filePath);
        }

        private Task<Stream> Read(string path)
        {
            var filePath = _environment.ResolvePath($"~/{_basePath}/{path}");

            return _fileSystem.ReadFile(filePath);
        }
    }
}