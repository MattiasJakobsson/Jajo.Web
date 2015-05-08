using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Spark;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace OpenWeb.Output.Spark
{
    public class ScanRequest
    {
        private readonly List<string> _roots;
        private readonly List<string> _filter;
        private readonly List<string> _excludes;
        private CompositeAction<FileFound> _onFound;

        public ScanRequest()
        {
            _roots = new List<string>();
            _filter = new List<string>();
            _excludes = new List<string>();
            _onFound = new CompositeAction<FileFound>();
        }

        public IEnumerable<string> Roots { get { return _roots; } }
        public string Filters { get { return string.Join(";", _filter); } }
        public IEnumerable<string> ExcludedDirectories { get { return _excludes; } }

        public void AddRoot(string root)
        {
            _roots.Add(root);
        }

        public void Include(string includeFilter)
        {
            _filter.Add(includeFilter);
        }
        public void ExcludeDirectory(string directoryPath)
        {
            _excludes.Add(directoryPath);
        }

        public void AddHandler(Action<FileFound> handler)
        {
            _onFound += handler;
        }

        public void OnFound(FileFound file)
        {
            _onFound.Do(file);
        }
    }
    public class CompositeAction<T, TU>
    {
        private readonly List<Action<T, TU>> _actions = new List<Action<T, TU>>();

        public static CompositeAction<T, TU> operator +(CompositeAction<T, TU> actions, Action<T, TU> action)
        {
            actions._actions.Add(action);
            return actions;
        }

        public void Do(T t, TU u)
        {
            _actions.ForEach(x => x(t, u));
        }
    }

    public class CompositeAction<T>
    {
        private readonly List<Action<T>> _actions = new List<Action<T>>();

        public static CompositeAction<T> operator +(CompositeAction<T> actions, Action<T> action)
        {
            actions._actions.Add(action);
            return actions;
        }

        public void Do(T target)
        {
            _actions.ForEach(x => x(target));
        }
    }

    public class FileFound
    {
        public FileFound(string path, string root, string directory)
        {
            Path = path;
            Root = root;
            Directory = directory;
        }

        public string Path { get; private set; }
        public string Root { get; private set; }
        public string Directory { get; private set; }

        public string GetFileName()
        {
            return System.IO.Path.GetFileName(Path);
        }
    }
    public class FileSystem
    {
        public const int BufferSize = 32768;

        public void CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                return;
            }

            dir.Create();
        }

        public long FileSizeOf(string path)
        {
            return new FileInfo(path).Length;
        }

        public bool IsFile(string path)
        {
            path = Path.GetFullPath(path);

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new IOException(string.Format("This path '{0}' doesn't exist!", path));
            }

            var attr = File.GetAttributes(path);

            return (attr & FileAttributes.Directory) != FileAttributes.Directory;
        }

        public bool FileExists(string filename)
        {
            return File.Exists(filename);
        }

        public void WriteStreamToFile(string filename, Stream stream)
        {
            CreateDirectory(Path.GetDirectoryName(filename));

            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                int bytesRead;
                var buffer = new byte[BufferSize];
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                } while (bytesRead > 0);
                fileStream.Flush();
            }
        }

        public void WriteStringToFile(string filename, string text)
        {
            CreateDirectory(Path.GetDirectoryName(filename));

            File.WriteAllText(filename, text);
        }

        public void AppendStringToFile(string filename, string text)
        {
            File.AppendAllText(filename, text);
        }


        public string ReadStringFromFile(string filename)
        {
            return File.ReadAllText(filename);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public void AlterFlatFile(string path, Action<List<string>> alteration)
        {
            var list = new List<string>();

            if (FileExists(path))
            {
                ReadTextFile(path, list.Add);
            }

            list.RemoveAll(x => x.Trim() == string.Empty);

            alteration(list);

            using (var writer = new StreamWriter(path))
            {
                list.ForEach(writer.WriteLine);
            }
        }

        public void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        public void CleanDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory)) return;


            DeleteDirectory(directory);
            Thread.Sleep(10);

            CreateDirectory(directory);
        }

        public bool DirectoryExists(string directory)
        {
            return Directory.Exists(directory);
        }

        public void DeleteFile(string filename)
        {
            if (!File.Exists(filename)) return;

            File.Delete(filename);
        }

        public void MoveFile(string from, string to)
        {
            CreateDirectory(Path.GetDirectoryName(to));

            try
            {
                File.Move(from, to);
            }
            catch (IOException ex)
            {
                var msg = string.Format("Trying to move '{0}' to '{1}'", from, to);
                throw new Exception(msg, ex);
            }
        }

        public void MoveFiles(string from, string to)
        {
            var files = Directory.GetFiles(from, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var partialPath = file.Replace(from, "");
                if (partialPath.StartsWith(@"\")) partialPath = partialPath.Substring(1);
                var newPath = Combine(to, partialPath);
                MoveFile(file, newPath);
            }
        }

        public void MoveDirectory(string from, string to)
        {
            Directory.Move(from, to);
        }

        public IEnumerable<string> ChildDirectoriesFor(string directory)
        {
            if (Directory.Exists(directory))
            {
                return Directory.GetDirectories(directory);
            }

            return new string[0];
        }

        public void ReadTextFile(string path, Action<string> callback)
        {
            if (!FileExists(path)) return;

            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    callback(line.Trim());
                }
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

        public static string Combine(params string[] paths)
        {
            return paths.Aggregate(Path.Combine);
        }

        public void LaunchBrowser(string filename)
        {
            Process.Start("explorer", filename);
        }

        public static IEnumerable<string> GetChildDirectories(string directory)
        {
            if (!Directory.Exists(directory))
                return new string[0];


            return Directory.GetDirectories(directory);
        }
    }
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

    public class FileScanner
    {
        private readonly FileSystem _fileSystem;
        private IList<string> _scannedDirectories;

        public FileScanner(FileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Scan(ScanRequest request)
        {
            var fileSet = new FileSet { Include = request.Filters, DeepSearch = false };
            _scannedDirectories = new List<string>();

            foreach (var root in request.Roots)
                Scan(root, root, fileSet, request.OnFound, request.ExcludedDirectories);
        }

        private void Scan(string root, string directory, FileSet fileSet, Action<FileFound> onFound, IEnumerable<string> excludes)
        {
            if (AlreadyScannedOrNonexistent(directory))
                return;

            _scannedDirectories.Add(directory);

            var excludeList = excludes.ToList();

            foreach (var childDirectory in _fileSystem.ChildDirectoriesFor(directory))
                Scan(root, childDirectory, fileSet, onFound, excludeList);

            var included = fileSet.IncludedFilesFor(directory).ToList();
            var excluded = included.Where(x => excludeList.Any(x.Contains));

            var files = included.Except(excluded).ToList();

            files.ForEach(file => onFound(new FileFound(file, root, directory)));
        }

        private bool AlreadyScannedOrNonexistent(string path)
        {
            return _scannedDirectories.Contains(path) || !_fileSystem.DirectoryExists(path);
        }
    }

    public class RenderOutputUsingSpark : IRenderOutput
    {
        private const string ViewModelTypePattern = @"<viewdata model=""([a-z&auml;&aring;&ouml;A-Z&Auml;&Aring;&Ouml;0-9!&amp;%/\(\)=\?;:\-_ \t\.\,]*)""";

        private readonly FileScanner _fileScanner;
        private readonly ISparkViewEngine _engine;
        private readonly UseMasterGrammar _grammar;

        private readonly CompositeAction<ScanRequest> _requestConfig = new CompositeAction<ScanRequest>();

        public RenderOutputUsingSpark(FileScanner fileScanner, ISparkViewEngine engine)
        {
            _fileScanner = fileScanner;
            _engine = engine;
            _requestConfig += x => x.Include("*.spark");
            _grammar = new UseMasterGrammar(engine.Settings.Prefix);
        }

        public async Task<Stream> Render(IDictionary<string, object> environment)
        {
            var templates = new List<Template>();
            var assemblies = new List<Assembly>();

            templates.AddRange(FindEmbeddedTemplates(assemblies));
            templates.AddRange(FindTemplatesFromFileSystem(assemblies, environment));

            var matchingTemplates = templates.Where(x => x.ModelType == environment.GetOutput().GetType()).ToList();

            if (matchingTemplates.Count < 1) return new MemoryStream();

            var endpoint = environment.GetRouteInformation().RoutedTo;

            if (matchingTemplates.Count > 1)
                throw new Exception(
                    string.Format(
                        "More then one template was found for endpoint '{0}.{1}'.\nThe following templates were found: {2}",
                        endpoint.ReflectedType.Name, endpoint.Name,
                        string.Join(", ", templates.Select(x => x.Name))));

            var template = templates.Single();

            var descriptor = BuildDescriptor(template, true, null);

            var sparkViewEntry = _engine.CreateEntry(descriptor);

            var view = sparkViewEntry.CreateInstance() as OpenWebSparkView;

            if (view == null)
                return new MemoryStream();

            return await Task.Factory.StartNew(() =>
            {
                var output = new MemoryStream();
                var writer = new StreamWriter(output);

                view.Render(new ViewContext(environment.GetOutput()), writer);

                output.Position = 0;

                return output;
            });
        }

        private SparkViewDescriptor BuildDescriptor(Template template, bool searchForMaster, ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
            {
                TargetNamespace = GetNamespaceEncodedPathViewPath(GetFullPath(template))
            };

            descriptor.Templates.Add(GetFullPath(template).Replace("\\", "/"));

            if (searchForMaster && TrailingUseMasterName(descriptor) == null)
            {
                LocatePotentialTemplate(
                    PotentialDefaultMasterLocations(template),
                    descriptor.Templates,
                    null);
            }

            var trailingUseMaster = TrailingUseMasterName(descriptor);
            while (searchForMaster && !string.IsNullOrEmpty(trailingUseMaster))
            {
                if (!LocatePotentialTemplate(
                    PotentialMasterLocations(template, trailingUseMaster),
                    descriptor.Templates,
                    searchedLocations))
                {
                    return null;
                }
                trailingUseMaster = TrailingUseMasterName(descriptor);
            }

            return descriptor;
        }

        private bool LocatePotentialTemplate(
            IEnumerable<string> potentialTemplates,
            ICollection<string> descriptorTemplates,
            ICollection<string> searchedLocations)
        {
            var templatesList = potentialTemplates.ToList();

            var template = templatesList.FirstOrDefault(t => _engine.ViewFolder.HasView(t));
            if (template != null)
            {
                descriptorTemplates.Add(template);
                return true;
            }

            if (searchedLocations != null)
            {
                foreach (var potentialTemplate in templatesList)
                {
                    searchedLocations.Add(potentialTemplate);
                }
            }

            return false;
        }

        protected virtual IEnumerable<string> PotentialMasterLocations(Template template, string masterName)
        {
            return new[]
            {
                string.Format("Layouts{0}{1}.spark", template.PathSeperator, masterName),
                string.Format("Shared{0}{1}.spark", template.PathSeperator, masterName)
            };
        }

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(Template template)
        {
            return new[]
            {
                string.Format("Layouts{0}Application.spark", template.PathSeperator),
                string.Format("Shared{0}Application.spark", template.PathSeperator)
            };
        }

        public string TrailingUseMasterName(SparkViewDescriptor descriptor)
        {
            var lastTemplate = descriptor.Templates.Last();
            var sourceContext = AbstractSyntaxProvider.CreateSourceContext(lastTemplate, _engine.ViewFolder);

            if (sourceContext == null)
            {
                return null;
            }

            var result = _grammar.ParseUseMaster(new Position(sourceContext));

            return result == null ? null : result.Value;
        }


        private static string GetFullPath(Template viewTemplate)
        {
            return string.Format("{0}{1}", viewTemplate.Path, viewTemplate.Name);
        }

        private static string GetNamespaceEncodedPathViewPath(string viewPath)
        {
            return viewPath.Replace('\\', '_');
        }

        private IEnumerable<Template> FindEmbeddedTemplates(ICollection<Assembly> assemblies)
        {
            var resources = GetResourcesFromAssemblies(assemblies);

            return resources.Select(x => GetTemplate(x, assemblies));
        }

        private IEnumerable<Template> FindTemplatesFromFileSystem(IEnumerable<Assembly> assemblies, IDictionary<string, object> environment)
        {
            var templates = new List<Template>();

            var request = BuildRequest(templates, assemblies, environment, environment.GetApplicationBasePath());

            _fileScanner.Scan(request);

            return templates;
        }

        private ScanRequest BuildRequest(ICollection<Template> templates, IEnumerable<Assembly> assemblies, IDictionary<string, object> environment, params string[] roots)
        {
            var request = new ScanRequest();
            _requestConfig.Do(request);

            roots.ToList().ForEach(request.AddRoot);
            request.AddHandler(fileFound => templates.Add(GetTemplate(fileFound, environment.GetApplicationBasePath(), assemblies)));

            return request;
        }

        private static IEnumerable<EmbeddedResource> GetResourcesFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(x => x.GetManifestResourceNames().Where(y => y.EndsWith(".spark")).Select(y => new EmbeddedResource(y, x)));
        }

        private Template GetTemplate(EmbeddedResource resource, IEnumerable<Assembly> availableAssemblies)
        {
            Func<TextReader> getContentReader = () => new StreamReader(resource.Assembly.GetManifestResourceStream(resource.GetResourceName()));

            return new Template(resource.GetViewName(),
                                resource.GetPath(),
                                ".",
                                FindModelType(getContentReader(), availableAssemblies),
                                getContentReader);
        }

        private Template GetTemplate(FileFound fileFound, string root, IEnumerable<Assembly> availableAssemblies)
        {
            Func<TextReader> getContentReader = () => new StreamReader(fileFound.Path);

            var fullDirectory = fileFound.Directory + "\\";
            var applicationPath = fullDirectory.Replace(root, string.Empty).Substring(1);

            return new Template(fileFound.GetFileName(),
                                applicationPath,
                                "\\",
                                FindModelType(getContentReader(), availableAssemblies),
                                getContentReader);
        }

        private static Type FindModelType(TextReader contentReader, IEnumerable<Assembly> availableAssemblies)
        {
            var content = contentReader.ReadToEnd();

            var match = Regex.Match(content, ViewModelTypePattern);

            if (!match.Success) return null;

            var captured = match.Captures.OfType<Capture>().FirstOrDefault();

            if (captured == null) return null;

            var capturedString = captured.Value.Replace("<viewdata model=\"", string.Empty).Replace("\"", string.Empty);

            var assembly = availableAssemblies.FirstOrDefault(x => x.GetType(capturedString, false) != null);

            if (assembly == null) return null;

            return assembly.GetType(capturedString, true);
        }

        private class EmbeddedResource
        {
            private readonly string _resourceName;

            public EmbeddedResource(string resourceName, Assembly assembly)
            {
                Assembly = assembly;
                _resourceName = resourceName;
            }

            public Assembly Assembly { get; private set; }

            public string GetPath()
            {
                return _resourceName.Replace(GetViewName(), string.Empty);
            }

            public string GetViewName()
            {
                var splitted = _resourceName.Split('.');

                return string.Format("{0}.{1}", splitted[splitted.Length - 2], splitted.Last());
            }

            public string GetResourceName()
            {
                return _resourceName;
            }
        }

        private class UseMasterGrammar : CharGrammar
        {
            public UseMasterGrammar(string prefix)
            {
                var whiteSpace0 = Rep(Ch(char.IsWhiteSpace));
                var whiteSpace1 = Rep1(Ch(char.IsWhiteSpace));
                var startOfElement = !string.IsNullOrEmpty(prefix) ? Ch("<" + prefix + ":use") : Ch("<use");
                var startOfAttribute = Ch("master").And(whiteSpace0).And(Ch('=')).And(whiteSpace0);
                var attrValue = Ch('\'').And(Rep(ChNot('\''))).And(Ch('\''))
                    .Or(Ch('\"').And(Rep(ChNot('\"'))).And(Ch('\"')));

                var endOfElement = Ch("/>");

                var useMaster = startOfElement
                    .And(whiteSpace1)
                    .And(startOfAttribute)
                    .And(attrValue)
                    .And(whiteSpace0)
                    .And(endOfElement)
                    .Build(hit => new string(hit.Left.Left.Down.Left.Down.ToArray()));

                ParseUseMaster =
                    pos =>
                    {
                        for (var scan = pos; scan.PotentialLength() != 0; scan = scan.Advance(1))
                        {
                            var result = useMaster(scan);
                            if (result != null)
                                return result;
                        }
                        return null;
                    };
            }

            public ParseAction<string> ParseUseMaster { get; private set; }
        }
    }

    public class Template
    {
        public Template(string name, string path, string pathSeperator, Type modelType, Func<TextReader> contents)
        {
            Name = name;
            Path = path;
            PathSeperator = pathSeperator;
            ModelType = modelType;
            Contents = contents;
        }

        public string Path { get; private set; }
        public string PathSeperator { get; private set; }
        public Func<TextReader> Contents { get; private set; }
        public Type ModelType { get; private set; }
        public string Name { get; private set; }
    }
}