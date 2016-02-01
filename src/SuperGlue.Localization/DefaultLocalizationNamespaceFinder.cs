using System.Collections.Generic;

namespace SuperGlue.Localization
{
    public class DefaultLocalizationNamespaceFinder : IFindCurrentLocalizationNamespace
    {
        public string Find(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput();

            if (output == null)
                return "";

            var type = output.GetType();

            var typeNamespace = type.Namespace;

            if (string.IsNullOrEmpty(typeNamespace))
                return type.Name;

            typeNamespace = typeNamespace.Replace(".", ":");

            return $"{typeNamespace}:{type.Name}";
        }
    }
}