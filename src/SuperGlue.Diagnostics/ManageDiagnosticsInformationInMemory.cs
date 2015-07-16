using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Diagnostics
{
    public class ManageDiagnosticsInformationInMemory : IManageDiagnosticsInformation
    {
        private static readonly IDictionary<string, ConcurrentLruLSet<DiagnosticsData>> Messurements = new ConcurrentDictionary<string, ConcurrentLruLSet<DiagnosticsData>>();
        private readonly IDictionary<string, object> _environment;

        public ManageDiagnosticsInformationInMemory(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public Task AddDiagnostics(string type, DiagnosticsData data)
        {
            var loweredType = (type ?? "").ToSlug().ToLower();

            if (!Messurements.ContainsKey(loweredType))
                Messurements[loweredType] = new ConcurrentLruLSet<DiagnosticsData>(100);

            Messurements[loweredType].Push(data);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetTypes()
        {
            var settings = _environment.GetSettings<DiagnosticsSettings>();

            return Task.FromResult(Messurements.Select(x => x.Key).Where(x => settings.IsKeyAllowed(x)));
        }

        public Task<IEnumerable<string>> GetKeysFor(string type)
        {
            var loweredType = (type ?? "").ToLower();

            var settings = _environment.GetSettings<DiagnosticsSettings>();

            return !settings.IsKeyAllowed(loweredType) ? Task.FromResult(Enumerable.Empty<string>()) : Task.FromResult(!Messurements.ContainsKey(loweredType) ? Enumerable.Empty<string>() : Messurements[loweredType].Select(x => x.Key));
        }

        public Task<IReadOnlyDictionary<string, IDiagnosticsValue>> GetDataFor(string type, string key)
        {
            var loweredType = (type ?? "").ToLower();
            var loweredKey = (key ?? "").ToLower();

            var settings = _environment.GetSettings<DiagnosticsSettings>();

            if (!settings.IsKeyAllowed(loweredType) || !Messurements.ContainsKey(loweredType))
                return Task.FromResult<IReadOnlyDictionary<string, IDiagnosticsValue>>(new Dictionary<string, IDiagnosticsValue>());

            return Task.FromResult<IReadOnlyDictionary<string, IDiagnosticsValue>>(Messurements[loweredType]
                .Where(x => x.Key == loweredKey)
                .SelectMany(x => x.Data)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Value).First()));
        }
    }
}