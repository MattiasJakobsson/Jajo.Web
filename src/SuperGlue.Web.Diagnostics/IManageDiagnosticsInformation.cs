using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Diagnostics
{
    public interface IManageDiagnosticsInformation
    {
        void RouteExecuted(object routedTo, TimeSpan executionTime);
        void UrlVisited(Uri url, TimeSpan executionTime);

        IDictionary<string, TimeSpan> GetRoutesWithAverageExecutionTime();
        IDictionary<string, TimeSpan> GetUrlsWithAverageExecutionTime();
    }
}