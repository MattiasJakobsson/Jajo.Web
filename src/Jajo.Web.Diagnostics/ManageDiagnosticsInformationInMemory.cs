using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Jajo.Web.Diagnostics
{
    public class ManageDiagnosticsInformationInMemory : IManageDiagnosticsInformation
    {
        private readonly IDictionary<string, ICollection<TimeSpan>> _routeExecutionTimes = new ConcurrentDictionary<string, ICollection<TimeSpan>>();
        private readonly IDictionary<string, ICollection<TimeSpan>> _urlExecutionTimes = new ConcurrentDictionary<string, ICollection<TimeSpan>>();

        public void RouteExecuted(object routedTo, TimeSpan executionTime)
        {
            if(!_routeExecutionTimes.ContainsKey(routedTo.ToString()))
                _routeExecutionTimes[routedTo.ToString()] = new List<TimeSpan>();

            _routeExecutionTimes[routedTo.ToString()].Add(executionTime);
        }

        public void UrlVisited(Uri url, TimeSpan executionTime)
        {
            if (!_urlExecutionTimes.ContainsKey(url.ToString()))
                _urlExecutionTimes[url.ToString()] = new List<TimeSpan>();

            _urlExecutionTimes[url.ToString()].Add(executionTime);
        }

        public IDictionary<string, TimeSpan> GetRoutesWithAverageExecutionTime()
        {
            return _routeExecutionTimes.ToDictionary(x => x.Key, x => TimeSpan.FromMilliseconds(x.Value.Average(y => y.TotalMilliseconds)));
        }

        public IDictionary<string, TimeSpan> GetUrlsWithAverageExecutionTime()
        {
            return _urlExecutionTimes.ToDictionary(x => x.Key, x => TimeSpan.FromMilliseconds(x.Value.Average(y => y.TotalMilliseconds)));
        }
    }
}