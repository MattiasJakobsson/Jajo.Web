using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Diagnostics
{
    public interface IManageDiagnosticsInformation
    {
        Task AddDiagnostics(string type, DiagnosticsData data);
        Task<IEnumerable<string>> GetTypes();
        Task<IEnumerable<string>> GetKeysFor(string type);
        Task<IReadOnlyDictionary<string, IDiagnosticsValue>> GetDataFor(string type, string key);
    }
}