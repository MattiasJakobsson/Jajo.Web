using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Diagnostics;

namespace SuperGlue.Web.Diagnostics.Endpoints
{
    public class Data
    {
        private readonly IManageDiagnosticsInformation _manageDiagnosticsInformation;

        public Data(IManageDiagnosticsInformation manageDiagnosticsInformation)
        {
            _manageDiagnosticsInformation = manageDiagnosticsInformation;
        }

        public async Task<DataQueryResult> Query(DataQueryInput input)
        {
            var data = await _manageDiagnosticsInformation.GetDataFor(input.Slug, input.Id, input.Step).ConfigureAwait(false);
            
            return new DataQueryResult(input.Slug, input.Id, input.Step, data.ToDictionary(x => x.Key, x => x.Value.Select(y => new KeyValuePair<string, string>(y.Key, y.Value.GetStringRepresentation()))));
        }
    }

    public class DataQueryInput
    {
        public string Slug { get; set; }
        public string Id { get; set; }
        public string Step { get; set; }
    }

    public class DataQueryResult
    {
        public DataQueryResult(string category, string type, string step, IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>> data)
        {
            Category = category;
            Type = type;
            Step = step;
            Data = data;
        }

        public string Category { get; private set; }
        public string Type { get; private set; }
        public string Step { get; private set; }
        public IReadOnlyDictionary<string, IEnumerable<KeyValuePair<string, string>>> Data { get; private set; }
    }
}