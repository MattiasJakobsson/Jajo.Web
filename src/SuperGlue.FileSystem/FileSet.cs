using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SuperGlue.FileSystem
{
    public class FileSet
    {
        [XmlAttribute]
        public string Include { get; set; }

        [XmlAttribute]
        public string Exclude { get; set; }

        public bool DeepSearch { get; set; }

        public FileSet()
        {
            Include = "*.*";
            DeepSearch = true;
        }

        public static FileSet Deep(string include, string exclude = null)
        {
            return new FileSet()
            {
                DeepSearch = true,
                Exclude = exclude,
                Include = include
            };
        }

        public static FileSet Shallow(string include, string exclude = null)
        {
            return new FileSet()
            {
                DeepSearch = false,
                Exclude = exclude,
                Include = include
            };
        }

        public void AppendInclude(string include)
        {
            if (Include == "*.*")
                Include = string.Empty;

            if (string.IsNullOrEmpty(Include))
            {
                Include = include;
            }
            else
            {
                var fileSet = this;
                var str = fileSet.Include + ";" + include;
                fileSet.Include = str;
            }
        }

        public void AppendExclude(string exclude)
        {
            if (string.IsNullOrEmpty(Exclude))
            {
                Exclude = exclude;
            }
            else
            {
                var fileSet = this;
                var str = fileSet.Exclude + ";" + exclude;
                fileSet.Exclude = str;
            }
        }

        public IEnumerable<string> IncludedFilesFor(string path)
        {
            return !new DirectoryInfo(path).Exists ? new string[0] : GetAllDistinctFiles(path, string.IsNullOrEmpty(Include) ? "*.*" : Include);
        }

        private IEnumerable<string> GetAllDistinctFiles(string path, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return new string[0];

            return pattern.Split(';').SelectMany(x =>
            {
                var str = path;
                var strArray = x.Replace("\\", "/").Split('/');
                var searchPattern = x;

                if (strArray.Length > 1)
                {
                    string path2 = string.Join(Path.DirectorySeparatorChar.ToString(), strArray.Take(strArray.Length - 1));
                    str = Path.Combine(str, path2);
                    searchPattern = strArray.Last();
                }

                var directoryInfo = new DirectoryInfo(str);
                var searchOption = DeepSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                try
                {
                    return directoryInfo.Exists ? (IEnumerable<string>)Directory.GetFiles(str, searchPattern, searchOption) : (IEnumerable<string>)new string[0];
                }
                catch (DirectoryNotFoundException)
                {
                    return (IEnumerable<string>)new string[0];
                }
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
                Include = string.Join(";", assemblyNames.OrderBy(x => x).Select(x => $"{x}.dll;{x}.exe"))
            };
        }

        public static FileSet ForAssemblyDebugFiles(IEnumerable<string> assemblyNames)
        {
            return new FileSet
            {
                DeepSearch = false,
                Exclude = null,
                Include = string.Join(";", assemblyNames.OrderBy(x => x).Select(x => $"{x}.pdb"))
            };
        }

        public bool Equals(FileSet other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Equals(other.Include, Include) && Equals(other.Exclude, Exclude))
                return other.DeepSearch.Equals(DeepSearch);

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != typeof(FileSet))
                return false;

            return Equals((FileSet)obj);
        }

        public override int GetHashCode()
        {
            return ((Include?.GetHashCode() ?? 0) * 397 ^ (Exclude?.GetHashCode() ?? 0)) * 397 ^ DeepSearch.GetHashCode();
        }

        public override string ToString()
        {
            return $"Include: {Include}, Exclude: {Exclude}";
        }

        public static FileSet Everything()
        {
            return new FileSet()
            {
                DeepSearch = true,
                Include = "*.*"
            };
        }
    }
}