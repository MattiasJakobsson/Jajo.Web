using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Diagnostics;
using SuperGlue.Web.RouteInputValidator;

namespace SuperGlue.Web.Diagnostics.Endpoints
{
    public class Information : IEnsureExists<InformationQueryInput>
    {
        private readonly IManageDiagnosticsInformation _manageDiagnosticsInformation;

        public Information(IManageDiagnosticsInformation manageDiagnosticsInformation)
        {
            _manageDiagnosticsInformation = manageDiagnosticsInformation;
        }

        public async Task<InformationQueryResult> Query(InformationQueryInput input)
        {
            var data = await _manageDiagnosticsInformation.GetDataFor(input.Slug, input.Id);
            
            return new InformationQueryResult(input.Slug, input.Id, data.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.GetStringRepresentation())));
        }

        public async Task<bool> Exists(InformationQueryInput input)
        {
            return (await _manageDiagnosticsInformation.GetDataFor(input.Slug, input.Id)).Any();
        }
    }

    public class InformationQueryInput
    {
        public string Slug { get; set; }
        public string Id { get; set; }
    }

    public class InformationQueryResult
    {
        public InformationQueryResult(string type, string key, IEnumerable<KeyValuePair<string, string>> data)
        {
            Type = type;
            Key = key;
            Data = data;
        }

        public string Type { get; private set; }
        public string Key { get; private set; }
        public IEnumerable<KeyValuePair<string, string>> Data { get; private set; }
    }
}