using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SuperGlue.Web.Output.Spark
{
    public class FileScanner
    {
        private IList<string> _scannedDirectories;

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

            var childDirectories = Directory.Exists(directory) ? Directory.GetDirectories(directory) : new string[0];

            foreach (var childDirectory in childDirectories)
                Scan(root, childDirectory, fileSet, onFound, excludeList);

            var included = fileSet.IncludedFilesFor(directory).ToList();
            var excluded = included.Where(x => excludeList.Any(x.Contains));

            var files = included.Except(excluded).ToList();

            files.ForEach(file => onFound(new FileFound(file, root, directory)));
        }

        private bool AlreadyScannedOrNonexistent(string path)
        {
            return _scannedDirectories.Contains(path) || !Directory.Exists(path);
        }
    }
}