using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.Output.Spark
{
    public class FileSystemTemplateSource : BaseTemplateSource
    {
        private readonly IEnumerable<Assembly> _assemblies;
        private readonly FileScanner _fileScanner;
        private readonly IEnumerable<string> _extraPaths;

        public FileSystemTemplateSource(IEnumerable<Assembly> assemblies, FileScanner fileScanner, IEnumerable<string> extraPaths)
        {
            _assemblies = assemblies;
            _fileScanner = fileScanner;
            _extraPaths = extraPaths;
        }

        public override IEnumerable<Template> FindTemplates()
        {
            var templates = new List<Template>();

            var paths = _extraPaths.Select(Path.GetFullPath).ToList();
            paths.Add(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            
            var request = BuildRequest(templates, _assemblies, paths.ToArray());

            _fileScanner.Scan(request);

            return templates;
        }

        private static Template GetTemplate(FileFound fileFound, IEnumerable<Assembly> availableAssemblies)
        {
            Func<TextReader> getContentReader = () => new StreamReader(fileFound.Path);

            var fullDirectory = fileFound.Directory + "\\";
            //var applicationPath = fullDirectory.Replace(fileFound.Root, string.Empty).Substring(1);

            return new Template(fileFound.GetFileName(),
                fullDirectory,
                "\\",
                FindModelType(getContentReader(), availableAssemblies),
                getContentReader);
        }

        private static ScanRequest BuildRequest(ICollection<Template> templates, IEnumerable<Assembly> assemblies, params string[] roots)
        {
            var request = new ScanRequest();
            request.Include("*.spark");

            roots.ToList().ForEach(request.AddRoot);
            request.AddHandler(fileFound => templates.Add(GetTemplate(fileFound, assemblies)));

            return request;
        }
    }
}