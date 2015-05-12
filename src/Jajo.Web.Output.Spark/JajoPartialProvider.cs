using System.Collections.Generic;
using System.IO;
using Spark;

namespace Jajo.Web.Output.Spark
{
    public class JajoPartialProvider : IPartialProvider
    {
        public IEnumerable<string> GetPaths(string viewPath)
        {
            do
            {
                viewPath = Path.GetDirectoryName(viewPath);

                if (string.IsNullOrEmpty(viewPath))
                    break;

                yield return viewPath;
                yield return Path.Combine(viewPath, Constants.Shared);
            }
            while (!string.IsNullOrEmpty(viewPath));
        }
    }
}