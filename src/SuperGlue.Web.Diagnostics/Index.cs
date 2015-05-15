using System;
using System.Collections.Generic;
using SuperGlue.Diagnostics;

namespace SuperGlue.Web.Diagnostics
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
            return new IndexQueryResult(_manageDiagnosticsInformation.GetAllMessurements());
        }
    }

    public class IndexQueryInput
    {

    }

    public class IndexQueryResult
    {
        public IndexQueryResult(IReadOnlyDictionary<string, IReadOnlyDictionary<string, TimeSpan>> averageTimes)
        {
            AverageTimes = averageTimes;
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TimeSpan>> AverageTimes { get; private set; }
    }
}