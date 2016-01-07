using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Monitoring
{
    public class HeartBeatSettings
    {
        private readonly ICollection<Tuple<IMonitorHeartBeats, TimeSpan?>> _heartBeats = new List<Tuple<IMonitorHeartBeats, TimeSpan?>>();
        private TimeSpan _defaultInterval = TimeSpan.FromMinutes(10);

        public void HeartBeatTo(string url, TimeSpan? interval, string message = null)
        {
            _heartBeats.Add(new Tuple<IMonitorHeartBeats, TimeSpan?>(new PostHeartBeatToUrl(url, message), interval));
        }

        public void SetDefaultInterval(TimeSpan interval)
        {
            _defaultInterval = interval;
        }

        public IEnumerable<Tuple<IMonitorHeartBeats, TimeSpan>> GetHeartBeatUrls()
        {
            return _heartBeats.Select(x => new Tuple<IMonitorHeartBeats, TimeSpan>(x.Item1, x.Item2 ?? _defaultInterval));
        }
    }
}