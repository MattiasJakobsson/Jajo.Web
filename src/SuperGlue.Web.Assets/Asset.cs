using System.IO;

namespace SuperGlue.Web.Assets
{
    public class Asset
    {
        public Asset(string name, Stream content, int priority)
        {
            Name = name;
            Content = content;
            Priority = priority;
        }

        public string Name { get; private set; }
        public Stream Content { get; private set; }
        public int Priority { get; private set; }
    }
}