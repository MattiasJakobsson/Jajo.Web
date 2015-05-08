using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OpenWeb.Output.Spark
{
    public class FileSet
    {
        public FileSet()
        {
            Include = "*.*";
            DeepSearch = true;
        }

        public void AppendInclude(string include)
        {
            if (Include == "*.*")
            {
                Include = string.Empty;
            }

            if (string.IsNullOrEmpty(Include))
            {
                Include = include;
            }
            else
            {
                Include += ";" + include;
            }

        }

        public string Include { get; set; }

        public string Exclude { get; set; }

        public bool DeepSearch { get; set; }

        public void AppendExclude(string exclude)
        {
            if (string.IsNullOrEmpty(Exclude))
            {
                Exclude = exclude;
            }
            else
            {
                Exclude += ";" + exclude;
            }
        }

        public IEnumerable<string> IncludedFilesFor(string path)
        {
            var directory = new DirectoryInfo(path);

            return directory.Exists
                ? GetAllDistinctFiles(path, string.IsNullOrEmpty(Include) ? "*.*" : Include)
                : new string[0];
        }

        private IEnumerable<string> GetAllDistinctFiles(string path, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return new string[0];

            return pattern.Split(';').SelectMany(x =>
            {
                var fullPath = path;
                var dirParts = x.Split(Path.DirectorySeparatorChar);
                var filePattern = x;

                if (dirParts.Length > 1)
                {
                    var subFolder = string.Join(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), dirParts.Take(dirParts.Length - 1));
                    fullPath = Path.Combine(fullPath, subFolder);
                    filePattern = dirParts.Last();
                }

                var directory = new DirectoryInfo(fullPath);
                var searchOption = DeepSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                return directory.Exists
                    ? Directory.GetFiles(fullPath, filePattern, searchOption)
                    : new string[0];
            }).Distinct();
        }

        public IEnumerable<string> ExcludedFilesFor(string path)
        {
            return GetAllDistinctFiles(path, Exclude);
        }

        public static FileSet ForAssemblyNames(IEnumerable<string> assemblyNames)
        {
            return new FileSet
            {
                DeepSearch = false,
                Exclude = null,
                Include = string.Join(";", assemblyNames.OrderBy(x => x).Select(x => string.Format("{0}.dll;{0}.exe", (x))))
            };
        }

        public static FileSet ForAssemblyDebugFiles(IEnumerable<string> assemblyNames)
        {
            return new FileSet
            {
                DeepSearch = false,
                Exclude = null,
                Include = string.Join(";", assemblyNames.OrderBy(x => x).Select(x => string.Format("{0}.pdb", (x))))
            };
        }

        public bool Equals(FileSet other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Include, Include) && Equals(other.Exclude, Exclude) && other.DeepSearch.Equals(DeepSearch);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(FileSet)) return false;
            return Equals((FileSet)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Include != null ? Include.GetHashCode() : 0);
                result = (result * 397) ^ (Exclude != null ? Exclude.GetHashCode() : 0);
                result = (result * 397) ^ DeepSearch.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("Include: {0}, Exclude: {1}", Include, Exclude);
        }

        public static FileSet Everything()
        {
            return new FileSet { DeepSearch = true, Include = "*.*" };
        }
    }
}