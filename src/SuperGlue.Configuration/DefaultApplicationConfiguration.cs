using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Configuration
{
    public class DefaultApplicationConfiguration : IApplicationConfiguration
    {
        private readonly IEnumerable<IConfigurationSource> _sources;

        public DefaultApplicationConfiguration(IEnumerable<IConfigurationSource> sources)
        {
            _sources = sources;
        }

        public IEnumerable<string> GetAvailableSettingKeys()
        {
            return _sources.SelectMany(source => source.GetAvailableSettingKeys());
        }

        public string GetSetting(string key)
        {
            return _sources.Where(x => x.HasSetting(key)).Select(x => x.GetSetting(key)).FirstOrDefault();
        }

        public string GetConnectionString(string key)
        {
            return _sources.Where(x => x.HasConnectionString(key)).Select(x => x.GetConnectionString(key)).FirstOrDefault();
        }
    }
}