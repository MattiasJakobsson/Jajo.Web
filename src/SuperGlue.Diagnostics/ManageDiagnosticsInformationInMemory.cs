using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Diagnostics
{
    public class ManageDiagnosticsInformationInMemory : IManageDiagnosticsInformation
    {
        private readonly IDictionary<string, ConcurrentLruLSet<DiagnosticsData>> _messurements = new ConcurrentDictionary<string, ConcurrentLruLSet<DiagnosticsData>>();

        public void AddMessurement(string messurementKey, DiagnosticsData data)
        {
            if(!_messurements.ContainsKey(messurementKey))
                _messurements[messurementKey] = new ConcurrentLruLSet<DiagnosticsData>(100);

            _messurements[messurementKey].Push(data);
        }

        public IReadOnlyDictionary<string, IEnumerable<DiagnosticsData>> GetAllMessurements()
        {
            return new ConcurrentDictionary<string, IEnumerable<DiagnosticsData>>(_messurements.ToDictionary(x => x.Key, x => (IEnumerable<DiagnosticsData>)x.Value));
        }
    }
}