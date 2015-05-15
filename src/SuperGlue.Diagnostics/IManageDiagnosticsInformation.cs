using System;
using System.Collections.Generic;

namespace SuperGlue.Diagnostics
{
    public interface IManageDiagnosticsInformation
    {
        void AddMessurement(string messurementKey, string key, TimeSpan executionTime);
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, TimeSpan>> GetAllMessurements();
    }
}