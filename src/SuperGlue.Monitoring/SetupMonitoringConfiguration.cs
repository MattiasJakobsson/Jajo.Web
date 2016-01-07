using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Monitoring
{
    public class SetupMonitoringConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Monitoring", configureAction: settings =>
            {
                var heartbeatSettings = settings.WithSettings<HeartBeatSettings>();

                int interval;
                if (int.TryParse(ConfigurationManager.AppSettings["SuperGlue.Monitoring.DefaultInterval"] ?? "", out interval))
                    heartbeatSettings.SetDefaultInterval(TimeSpan.FromMinutes(interval));

                var urlKeys = ConfigurationManager.AppSettings.AllKeys.Where(x => x.StartsWith("SuperGlue.Monitoring.HeartBeatUrls.") && !x.EndsWith(".Interval") && !x.EndsWith(".Message")).ToList();

                foreach (var urlKey in urlKeys)
                {
                    var url = ConfigurationManager.AppSettings[urlKey];

                    if (string.IsNullOrEmpty(url))
                        continue;

                    int urlIntervalMinutes;
                    TimeSpan? urlInterval = null;

                    if (int.TryParse(ConfigurationManager.AppSettings[string.Format("{0}.Interval", urlKey)] ?? "", out urlIntervalMinutes))
                        urlInterval = TimeSpan.FromMinutes(urlIntervalMinutes);

                    var message = ConfigurationManager.AppSettings[string.Format("{0}.Message", urlKey)];

                    heartbeatSettings.HeartBeatTo(url, urlInterval, message);
                }

                return Task.CompletedTask;
            });
        }
    }
}