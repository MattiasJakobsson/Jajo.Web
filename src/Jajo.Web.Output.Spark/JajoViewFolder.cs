using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Jajo.Web.Output.Spark
{
    public class JajoViewFolder : IViewFolder
    {
        private readonly IEnumerable<Template> _viewTemplates;

        public JajoViewFolder(IEnumerable<Template> viewTemplates)
        {
            _viewTemplates = viewTemplates;
        }

        public IViewFile GetViewSource(string path)
        {
            var searchPath = ConvertPath(path);

            var viewTemplate = _viewTemplates
                .FirstOrDefault(v => CompareViewPaths(ConvertPath(GetFullPath(v)), searchPath));

            if (viewTemplate == null)
                throw new FileNotFoundException(string.Format("Template {0} not found", path), path);

            return new JajoViewFile(viewTemplate);
        }

        public IList<string> ListViews(string path)
        {
            if(path == null)
                return new List<string>();

            var views = _viewTemplates.
                Where(v => v.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                .Select(v => v.Path.Length == path.Length ? v.Name : v.Path.Substring(path.Length) + v.Name)
                .ToList();

            return views;
        }

        public bool HasView(string path)
        {
            var searchPath = ConvertPath(path);

            return _viewTemplates.Any(v => CompareViewPaths(GetFullPath(v), searchPath));
        }

        private static bool CompareViewPaths(string storedViewPath, string requestedViewPath)
        {
            return string.Equals(storedViewPath, requestedViewPath, StringComparison.OrdinalIgnoreCase);
        }

        private static string ConvertPath(string path)
        {
            return path.Replace(@"\", "/");
        }

        private static string GetFullPath(Template viewTemplate)
        {
            return string.Format("{0}{1}", viewTemplate.Path, viewTemplate.Name);
        }

        public class JajoViewFile : IViewFile
        {
            private readonly Template _viewTemplate;
            private readonly long _created;

            public JajoViewFile(Template viewTemplate)
            {
                _viewTemplate = viewTemplate;
                _created = DateTime.Now.Ticks;
            }

            public long LastModified
            {
                get { return _created; }
            }

            public Stream OpenViewStream()
            {
                using (var reader = _viewTemplate.Contents())
                {
                    return new MemoryStream(Encoding.UTF8.GetBytes(reader.ReadToEnd()));
                }
            }
        }
    }
}