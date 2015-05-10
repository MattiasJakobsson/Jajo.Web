using System.Reflection;

namespace OpenWeb.Configuration
{
    public class InitializedSubApplication
    {
        public InitializedSubApplication(string path, string name, Assembly assembly)
        {
            Path = path;
            Name = name;
            Assembly = assembly;
        }

        public string Path { get; private set; }
        public string Name { get; private set; }
        public Assembly Assembly { get; private set; }
    }
}