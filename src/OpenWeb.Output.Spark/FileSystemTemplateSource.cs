using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenWeb.Output.Spark
{
    public class FileSystemTemplateSource : BaseTemplateSource
    {
        private readonly IEnumerable<Assembly> _assemblies;
        private readonly FileScanner _fileScanner;

        public FileSystemTemplateSource(IEnumerable<Assembly> assemblies, FileScanner fileScanner)
        {
            _assemblies = assemblies;
            _fileScanner = fileScanner;
        }

        public override IEnumerable<Template> FindTemplates()
        {
            var templates = new List<Template>();

            var request = BuildRequest(templates, _assemblies, AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

            _fileScanner.Scan(request);

            return templates;
        }

        private static Template GetTemplate(FileFound fileFound, string root, IEnumerable<Assembly> availableAssemblies)
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

        private static ScanRequest BuildRequest(ICollection<Template> templates, IEnumerable<Assembly> assemblies, params string[] roots)
        {
            var request = new ScanRequest();
            request.Include("*.spark");

            roots.ToList().ForEach(request.AddRoot);
            request.AddHandler(fileFound => templates.Add(GetTemplate(fileFound, AppDomain.CurrentDomain.SetupInformation.ApplicationBase, assemblies)));

            return request;
        }
    }
}