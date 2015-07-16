using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Diagnostics;
using SuperGlue.Web.RouteInputValidator;

namespace SuperGlue.Web.Diagnostics.Endpoints
{
    public class Details : IEnsureExists<DetailsQueryInput>
    {
        private readonly IManageDiagnosticsInformation _manageDiagnosticsInformation;

        public Details(IManageDiagnosticsInformation manageDiagnosticsInformation)
        {
            _manageDiagnosticsInformation = manageDiagnosticsInformation;
        }

        public async Task<DetailsQueryResult> Query(DetailsQueryInput input)
        {
            var keys = await _manageDiagnosticsInformation.GetKeysFor(input.Slug);

            return new DetailsQueryResult(input.Slug, keys);
        }

        public async Task<bool> Exists(DetailsQueryInput input)
        {
            return (await _manageDiagnosticsInformation.GetKeysFor(input.Slug)).Any();
        }
    }

    public class DetailsQueryInput
    {
        public string Slug { get; set; }
    }

    public class DetailsQueryResult
    {
        public DetailsQueryResult(string type, IEnumerable<string> keys)
        {
            Type = type;
            Keys = keys;
        }

        public string Type { get; private set; }
        public IEnumerable<string> Keys { get; private set; }

        public InformationQueryInput GetInformationInput(string key)
        {
            return new InformationQueryInput
            {
                Slug = Type,
                Id = key
            };
        }
    }
}