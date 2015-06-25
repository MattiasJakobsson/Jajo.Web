using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SuperGlue.Web.Assets
{
    public class AssetSettings
    {
        private readonly IList<AssetsSource> _sources = new List<AssetsSource>();

        public bool AssetsSetupEnabled { get; private set; }
        public string AssetsDestination { get; private set; }
        public IReadOnlyCollection<AssetsSource> Sources { get { return new ReadOnlyCollection<AssetsSource>(_sources); } }

        public AssetSettings SetSetupEnabled(bool enabled)
        {
            AssetsSetupEnabled = enabled;
            return this;
        }

        public AssetSettings UseDestination(string destination)
        {
            AssetsDestination = destination;
            return this;
        }

        public AssetSettings AddSource(string source, int priority)
        {
            _sources.Add(new AssetsSource(source, priority));
            return this;
        }

        public class AssetsSource
        {
            public AssetsSource(string path, int priority)
            {
                Path = path;
                Priority = priority;
            }

            public string Path { get; private set; }
            public int Priority { get; private set; }
        }
    }
}