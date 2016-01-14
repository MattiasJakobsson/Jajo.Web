using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Output.Spark
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

        public IEnumerable<string> Roots => _roots;
        public string Filters => string.Join(";", _filter);
        public IEnumerable<string> ExcludedDirectories => _excludes;

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
}