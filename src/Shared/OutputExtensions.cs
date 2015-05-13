using System.Collections.Generic;

namespace SuperGlue.Web
{
    internal static class OutputExtensions
    {
        public static object GetOutput(this IDictionary<string, object> environment)
        {
            return environment.Get<object>("superglue.Output");
        }
    }
}