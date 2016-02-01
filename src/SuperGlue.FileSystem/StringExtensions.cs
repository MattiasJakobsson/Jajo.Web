using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SuperGlue.FileSystem
{
    public static class StringExtensions
    {
        public static string AppendPath(this string path, params string[] parts)
        {
            var list = new List<string>()
              {
                path
              };

            list.AddRange(parts);
            return FileSystem.Combine(list.ToArray());
        }

        public static string PathRelativeTo(this string path, string root)
        {
            var pathParts1 = GetPathParts(path);
            var pathParts2 = GetPathParts(root);
            var num = pathParts1.Count > pathParts2.Count ? pathParts2.Count : pathParts1.Count;

            for (var index = 0; index < num && pathParts1.First().Equals(pathParts2.First(), StringComparison.OrdinalIgnoreCase); ++index)
            {
                pathParts1.RemoveAt(0);
                pathParts2.RemoveAt(0);
            }

            for (var index = 0; index < pathParts2.Count; ++index)
                pathParts1.Insert(0, "..");

            return pathParts1.Count <= 0 ? string.Empty : FileSystem.Combine(pathParts1.ToArray());
        }

        public static IList<string> GetPathParts(this string path)
        {
            return path.Split(new[]
            {
                Path.DirectorySeparatorChar
            }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}