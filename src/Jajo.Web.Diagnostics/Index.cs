using System;
using System.Collections.Generic;

namespace Jajo.Web.Diagnostics
{
    public class Index
    {
        private readonly IManageDiagnosticsInformation _manageDiagnosticsInformation;

        public Index(IManageDiagnosticsInformation manageDiagnosticsInformation)
        {
            _manageDiagnosticsInformation = manageDiagnosticsInformation;
        }

        public IndexQueryResult Query(IndexQueryInput input)
        {
            var averageTimes = new Dictionary<string, IDictionary<string, TimeSpan>>();

            averageTimes["Routes"] = _manageDiagnosticsInformation.GetRoutesWithAverageExecutionTime();
            averageTimes["Urls"] = _manageDiagnosticsInformation.GetUrlsWithAverageExecutionTime();

            return new IndexQueryResult(averageTimes);
        }
    }

    public class IndexQueryInput
    {

    }

    public class IndexQueryResult
    {
        public IndexQueryResult(IDictionary<string, IDictionary<string, TimeSpan>> averageTimes)
        {
            AverageTimes = averageTimes;
        }

        public IDictionary<string, IDictionary<string, TimeSpan>> AverageTimes { get; private set; }
    }
}