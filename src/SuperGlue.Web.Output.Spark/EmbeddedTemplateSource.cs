using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.Output.Spark
{
    public class EmbeddedTemplateSource : BaseTemplateSource
    {
        private readonly IEnumerable<Assembly> _assemblies;

        public EmbeddedTemplateSource(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        public override IEnumerable<Template> FindTemplates()
        {
            var resources = GetResourcesFromAssemblies(_assemblies);

            return resources.Select(x => GetTemplate(x, _assemblies));
        }

        private static IEnumerable<EmbeddedResource> GetResourcesFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(x => x.GetManifestResourceNames().Where(y => y.EndsWith(".spark")).Select(y => new EmbeddedResource(y, x)));
        }

        private static Template GetTemplate(EmbeddedResource resource, IEnumerable<Assembly> availableAssemblies)
        {
            Func<TextReader> getContentReader = () =>
            {
                var stream = resource.Assembly.GetManifestResourceStream(resource.GetResourceName());

                return new StreamReader(stream ?? new MemoryStream());
            };

            return new Template(resource.GetViewName(),
                resource.GetPath(),
                ".",
                FindModelType(getContentReader(), availableAssemblies),
                getContentReader);
        }

        private class EmbeddedResource
        {
            private readonly string _resourceName;

            public EmbeddedResource(string resourceName, Assembly assembly)
            {
                Assembly = assembly;
                _resourceName = resourceName;
            }

            public Assembly Assembly { get; }

            public string GetPath()
            {
                return _resourceName.Replace(GetViewName(), string.Empty);
            }

            public string GetViewName()
            {
                var splitted = _resourceName.Split('.');

                return $"{splitted[splitted.Length - 2]}.{splitted.Last()}";
            }

            public string GetResourceName()
            {
                return _resourceName;
            }
        }

    }
}