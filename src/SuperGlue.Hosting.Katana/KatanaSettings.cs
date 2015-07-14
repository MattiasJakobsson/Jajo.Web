using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SuperGlue.Hosting.Katana
{
    public class KatanaSettings
    {
        private readonly IList<string> _urls = new List<string>();

        public KatanaSettings BindTo(string url)
        {
            _urls.Add(url);

            return this;
        }

        internal IReadOnlyCollection<string> GetBindings()
        {
            return new ReadOnlyCollection<string>(_urls);
        }
    }
}