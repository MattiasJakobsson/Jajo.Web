using System.Collections.Generic;

namespace SuperGlue.Diagnostics
{
    public interface IManageDiagnosticsInformation
    {
        void AddMessurement(string messurementKey, DiagnosticsData data);
        IReadOnlyDictionary<string, IEnumerable<DiagnosticsData>> GetAllMessurements();
    }
}