using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.FileSystem
{
    public interface IFileSystem
    {
        bool FileExists(string filename);

        Task DeleteFile(string filename);

        Task MoveFile(string from, string to);

        Task MoveDirectory(string from, string to);

        bool IsFile(string path);

        string GetFullPath(string path);

        long FileSizeOf(string path);

        Task Copy(string source, string destination);

        Task WriteStreamToFile(string filename, Stream stream);

        Task WriteStringToFile(string filename, string text);

        Task AppendStringToFile(string filename, string text);

        Task<string> ReadStringFromFile(string filename);

        Task WriteObjectToFile(string filename, object target);

        Task<T> LoadFromFile<T>(string filename) where T : new();

        Task<T> LoadFromFileOrThrow<T>(string filename) where T : new();

        Task CreateDirectory(string directory);

        Task DeleteDirectory(string directory);

        Task CleanDirectory(string directory);

        bool DirectoryExists(string directory);

        IEnumerable<string> ChildDirectoriesFor(string directory);

        IEnumerable<string> FindFiles(string directory, FileSet searchSpecification);

        Task ReadTextFile(string path, Func<string, Task> reader);

        Task MoveFiles(string from, string to);

        string GetDirectory(string path);

        string GetFileName(string path);

        Task AlterFlatFile(string path, Func<List<string>, Task> alteration);

        Task Copy(string source, string destination, CopyBehavior behavior);
    }
}