using System;
using System.IO;

namespace SuperGlue.Configuration
{
    public class SubApplication
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string AbsolutePath { get; set; }

        public string GetAbsolutePath(string basePath)
        {
            if (!string.IsNullOrEmpty(AbsolutePath))
                return AbsolutePath;

            var relativePath = RelativePath;
            var baseDirectory = new DirectoryInfo(basePath);

            while (relativePath.StartsWith("..\\"))
            {
                if(baseDirectory == null)
                    throw new Exception("Invalid path");

                baseDirectory = baseDirectory.Parent;

                relativePath = relativePath.Substring(3);
            }

            if (baseDirectory == null)
                throw new Exception("Invalid path");

            return Path.Combine(baseDirectory.FullName, relativePath);
        }
    }
}