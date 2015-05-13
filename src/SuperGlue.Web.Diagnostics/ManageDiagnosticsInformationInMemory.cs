using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Web.Diagnostics
{
    public class ManageDiagnosticsInformationInMemory : IManageDiagnosticsInformation
    {
        private readonly IDictionary<string, ConcurrentLruLSet<TimeSpan>> _routeExecutionTimes = new ConcurrentDictionary<string, ConcurrentLruLSet<TimeSpan>>();
        private readonly IDictionary<string, ConcurrentLruLSet<TimeSpan>> _urlExecutionTimes = new ConcurrentDictionary<string, ConcurrentLruLSet<TimeSpan>>();

        public void RouteExecuted(object routedTo, TimeSpan executionTime)
        {
            if(!_routeExecutionTimes.ContainsKey(routedTo.ToString()))
                _routeExecutionTimes[routedTo.ToString()] = new ConcurrentLruLSet<TimeSpan>(100);

            _routeExecutionTimes[routedTo.ToString()].Push(executionTime);
        }

        public void UrlVisited(Uri url, TimeSpan executionTime)
        {
            if (!_urlExecutionTimes.ContainsKey(url.ToString()))
                _urlExecutionTimes[url.ToString()] = new ConcurrentLruLSet<TimeSpan>(100);

            _urlExecutionTimes[url.ToString()].Push(executionTime);
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