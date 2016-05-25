using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SuperGlue.FileSystem
{
    public class FileSystem : IFileSystem
    {
        public const int BufferSize = 32768;

        public Task CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Task.CompletedTask;

            var directoryInfo = new DirectoryInfo(path);

            if (directoryInfo.Exists)
                return Task.CompletedTask;

            directoryInfo.Create();

            return Task.CompletedTask;
        }

        public long FileSizeOf(string path)
        {
            return new FileInfo(path).Length;
        }

        public Task Copy(string source, string destination)
        {
            return Copy(source, destination, CopyBehavior.Overwrite);
        }

        public Task Copy(string source, string destination, CopyBehavior behavior)
        {
            return IsFile(source) ? InternalFileCopy(source, destination, behavior) : InternalDirectoryCopy(source, destination, behavior);
        }

        public bool IsFile(string path)
        {
            path = Path.GetFullPath(path);

            if (!File.Exists(path) && !Directory.Exists(path))
                throw new IOException($"This path '{path}' doesn't exist!");

            return (File.GetAttributes(path) & FileAttributes.Directory) != FileAttributes.Directory;
        }

        public bool FileExists(string filename)
        {
            return File.Exists(filename);
        }

        public async Task WriteStreamToFile(string filename, Stream stream)
        {
            await CreateDirectory(Path.GetDirectoryName(filename)).ConfigureAwait(false);

            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                var buffer = new byte[32768];
                int count;
                do
                {
                    count = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                    if (count > 0)
                        await fileStream.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                }
                while (count > 0);
                await fileStream.FlushAsync().ConfigureAwait(false);
            }
        }

        public Task WriteStringToFile(string filename, string text)
        {
            CreateDirectory(Path.GetDirectoryName(filename));
            File.WriteAllText(filename, text);

            return Task.CompletedTask;
        }

        public Task AppendStringToFile(string filename, string text)
        {
            File.AppendAllText(filename, text);

            return Task.CompletedTask;
        }

        public Task<string> ReadStringFromFile(string filename)
        {
            return Task.FromResult(File.ReadAllText(filename));
        }

        public Task<Stream> ReadFile(string filename)
        {
            return Task.FromResult<Stream>(File.Open(filename, FileMode.Open));
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public async Task AlterFlatFile(string path, Func<List<string>, Task> alteration)
        {
            var list = new List<string>();
            if (FileExists(path))
                await ReadTextFile(path, x =>
                {
                    list.Add(x);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

            list.RemoveAll(x => x.Trim() == string.Empty);

            await alteration(list).ConfigureAwait(false);

            var writer = new StreamWriter(path);

            try
            {
                foreach (var item in list)
                    writer.WriteLine(item);
            }
            finally
            {
                writer.Dispose();
            }
        }

        public Task DeleteDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return Task.CompletedTask;

            Directory.Delete(directory, true);

            return Task.CompletedTask;
        }

        public async Task CleanDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                return;

            await DeleteDirectory(directory).ConfigureAwait(false);

            Thread.Sleep(10);

            await CreateDirectory(directory).ConfigureAwait(false);
        }

        public bool DirectoryExists(string directory)
        {
            return Directory.Exists(directory);
        }

        public async Task WriteObjectToFile(string filename, object target)
        {
            var xmlSerializer = new XmlSerializer(target.GetType());
            await CreateDirectory(GetDirectory(filename)).ConfigureAwait(false);

            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                xmlSerializer.Serialize(fileStream, target);
        }

        public Task<T> LoadFromFileOrThrow<T>(string filename) where T : new()
        {
            if (!FileExists(filename))
                throw new ApplicationException("Unable to deserialize the contents of file {0}. It does not exist or we do not have read access to it.");

            return LoadFromFile<T>(filename);
        }

        public Task<T> LoadFromFile<T>(string filename) where T : new()
        {
            if (!FileExists(filename))
                return Task.FromResult(new T());

            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    return Task.FromResult((T)xmlSerializer.Deserialize(fileStream));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Unable to deserialize the contents of file {filename} into an instance of type {typeof(T).FullName}", ex);
                }
            }
        }

        public Task DeleteFile(string filename)
        {
            if (!File.Exists(filename))
                return Task.CompletedTask;

            File.Delete(filename);

            return Task.CompletedTask;
        }

        public Task MoveFile(string from, string to)
        {
            CreateDirectory(Path.GetDirectoryName(to));

            try
            {
                File.Move(from, to);
            }
            catch (IOException ex)
            {
                throw new Exception($"Trying to move '{from}' to '{to}'", ex);
            }

            return Task.CompletedTask;
        }

        public async Task MoveFiles(string from, string to)
        {
            foreach (var from1 in Directory.GetFiles(from, "*.*", SearchOption.AllDirectories))
            {
                var str = from1.Replace(from, "");

                if (str.StartsWith("\\"))
                    str = str.Substring(1);

                var to1 = Combine(to, str);

                await MoveFile(from1, to1).ConfigureAwait(false);
            }
        }

        public Task MoveDirectory(string from, string to)
        {
            Directory.Move(from, to);

            return Task.CompletedTask;
        }

        public IEnumerable<string> ChildDirectoriesFor(string directory)
        {
            return Directory.Exists(directory) ? Directory.GetDirectories(directory) : new string[0];
        }

        public IEnumerable<string> FindFiles(string directory, FileSet searchSpecification)
        {
            var enumerable = searchSpecification.ExcludedFilesFor(directory);
            var list = searchSpecification.IncludedFilesFor(directory).ToList();
            list.RemoveAll(x => enumerable.Contains(x));

            return list;
        }

        public async Task ReadTextFile(string path, Func<string, Task> callback)
        {
            if (!FileExists(path))
                return;

            using (var streamReader = new StreamReader(path))
            {
                string str;

                while ((str = streamReader.ReadLine()) != null)
                    await callback(str.Trim()).ConfigureAwait(false);
            }
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public string GetDirectory(string path)
        {
            return Path.GetDirectoryName(path);
        }

        private async Task InternalFileCopy(string source, string destination, CopyBehavior behavior)
        {
            var fileName = Path.GetFileName(source);
            var fullPath = Path.GetFullPath(source);
            var str = Path.GetFullPath(destination);
            var flag = DestinationIsFile(destination);
            var path = str;

            if (flag)
                path = Path.GetDirectoryName(str);

            await CreateDirectory(path).ConfigureAwait(false);

            if (!flag)
                str = Combine(str, fileName);

            var overwrite = behavior == CopyBehavior.Overwrite;

            if (!overwrite)
            {
                if (FileExists(str))
                    return;
            }

            try
            {
                File.Copy(fullPath, str, overwrite);
            }
            catch (Exception ex)
            {
                throw new Exception($"Was trying to copy '{fullPath}' to '{str}' and encountered an error. :(", ex);
            }
        }

        private async Task InternalDirectoryCopy(string source, string destination, CopyBehavior behavior)
        {
            foreach (var file in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                var str = file.PathRelativeTo(source);

                await InternalFileCopy(file, destination.AppendPath(str), behavior).ConfigureAwait(false);
            }
        }

        private bool DestinationIsFile(string destination)
        {
            if (FileExists(destination) || DirectoryExists(destination))
                return IsFile(destination);

            return destination.Last() != Path.DirectorySeparatorChar;
        }

        public static string Combine(params string[] paths)
        {
            return paths.Aggregate(Path.Combine);
        }

        public static IEnumerable<string> GetChildDirectories(string directory)
        {
            return !Directory.Exists(directory) ? new string[0] : Directory.GetDirectories(directory);
        }
    }
}