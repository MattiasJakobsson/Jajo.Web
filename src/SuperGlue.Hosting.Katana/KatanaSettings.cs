using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.Hosting.Katana
{
    public class KatanaSettings
    {
        private readonly IList<string> _urls = new List<string>();
        private readonly IList<string> _fallbackUrls = new List<string>();

        public KatanaSettings BindTo(string url)
        {
            _urls.Add(url);

            return this;
        }

        public KatanaSettings FallbackTo(string url)
        {
            _fallbackUrls.Add(url);

            return this;
        }

        internal IReadOnlyCollection<string> GetBindings()
        {
            if(!_urls.Any())
                return new ReadOnlyCollection<string>(_fallbackUrls);

            return new ReadOnlyCollection<string>(_urls);
        }
    }
}