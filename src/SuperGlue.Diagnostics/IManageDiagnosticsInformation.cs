using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Diagnostics
{
    public interface IManageDiagnosticsInformation
    {
        Task AddDiagnostics(string category, string type, string step, DiagnosticsData data);
        Task<IEnumerable<string>> GetCategories();
        Task<IEnumerable<string>> GetTypesFor(string category);
        Task<IEnumerable<string>> GetStepsFor(string category, string type, int numberOfSteps = 50);
        Task<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>> GetDataFor(string category, string type, string step);
    }
}