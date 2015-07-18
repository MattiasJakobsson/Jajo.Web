using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpTest.Net.Collections;

namespace SuperGlue.Diagnostics
{
    public class ManageDiagnosticsInformationInMemory : IManageDiagnosticsInformation
    {
        private static readonly IDictionary<string, ConcurrentDictionary<string, LurchTable<string, ConcurrentBag<DiagnosticsData>>>> Data = new ConcurrentDictionary<string, ConcurrentDictionary<string, LurchTable<string, ConcurrentBag<DiagnosticsData>>>>();

        public Task AddDiagnostics(string category, string type, string step, DiagnosticsData data)
        {
            var loweredCategory = (category ?? "").ToSlug().ToLower();
            var loweredType = (type ?? "").ToSlug().ToLower();
            var loweredStep = (step ?? "").ToSlug().ToLower();

            if (!Data.ContainsKey(loweredCategory))
                Data[loweredCategory] = new ConcurrentDictionary<string, LurchTable<string, ConcurrentBag<DiagnosticsData>>>();

            if (!Data[loweredCategory].ContainsKey(loweredType))
                Data[loweredCategory][loweredType] = new LurchTable<string, ConcurrentBag<DiagnosticsData>>(50);

            if (!Data[loweredCategory][loweredType].ContainsKey(loweredStep))
                Data[loweredCategory][loweredType][loweredStep] = new ConcurrentBag<DiagnosticsData>();

            Data[loweredCategory][loweredType][loweredStep].Add(data);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetCategories()
        {
            return Task.FromResult<IEnumerable<string>>(Data.Keys);
        }

        public Task<IEnumerable<string>> GetTypesFor(string category)
        {
            var loweredCategory = (category ?? "").ToSlug().ToLower();

            ConcurrentDictionary<string, LurchTable<string, ConcurrentBag<DiagnosticsData>>> categoryData;

            return Task.FromResult(!Data.TryGetValue(loweredCategory, out categoryData) ? Enumerable.Empty<string>() : categoryData.Keys);
        }

        public Task<IEnumerable<string>> GetStepsFor(string category, string type, int numberOfSteps = 50)
        {
            var loweredCategory = (category ?? "").ToSlug().ToLower();
            var loweredType = (type ?? "").ToSlug().ToLower();

            ConcurrentDictionary<string, LurchTable<string, ConcurrentBag<DiagnosticsData>>> categoryData;

            if (!Data.TryGetValue(loweredCategory, out categoryData))
                return Task.FromResult(Enumerable.Empty<string>());

            LurchTable<string, ConcurrentBag<DiagnosticsData>> typeData;

            return Task.FromResult(!categoryData.TryGetValue(loweredType, out typeData) ? Enumerable.Empty<string>() : typeData.Keys.Take(numberOfSteps));
        }

        public Task<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>> GetDataFor(string category, string type, string step)
        {
            var loweredCategory = (category ?? "").ToSlug().ToLower();
            var loweredType = (type ?? "").ToSlug().ToLower();
            var loweredStep = (step ?? "").ToSlug().ToLower();

            ConcurrentDictionary<string, LurchTable<string, ConcurrentBag<DiagnosticsData>>> categoryData;

            if (!Data.TryGetValue(loweredCategory, out categoryData))
                return Task.FromResult<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>>(new Dictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>());

            LurchTable<string, ConcurrentBag<DiagnosticsData>> typeData;

            if (!categoryData.TryGetValue(loweredType, out typeData))
                return Task.FromResult<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>>(new Dictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>());

            ConcurrentBag<DiagnosticsData> stepData;

            if (!typeData.TryGetValue(loweredStep, out stepData))
                return Task.FromResult<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>>(new Dictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>());

            var result = stepData
                .GroupBy(x => x.Key, x => x.Data)
                .ToDictionary(x => x.Key, x => x.SelectMany(y => y));

            return Task.FromResult<IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, IDiagnosticsValue>>>>(result);
        }
    }
}