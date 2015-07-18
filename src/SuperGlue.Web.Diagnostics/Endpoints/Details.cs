using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Diagnostics;

namespace SuperGlue.Web.Diagnostics.Endpoints
{
    public class Details
    {
        private readonly IManageDiagnosticsInformation _manageDiagnosticsInformation;

        public Details(IManageDiagnosticsInformation manageDiagnosticsInformation)
        {
            _manageDiagnosticsInformation = manageDiagnosticsInformation;
        }

        public async Task<DetailsQueryResult> Query(DetailsQueryInput input)
        {
            var types = await _manageDiagnosticsInformation.GetTypesFor(input.Slug);

            return new DetailsQueryResult(input.Slug, types);
        }
    }

    public class DetailsQueryInput
    {
        public string Slug { get; set; }
    }

    public class DetailsQueryResult
    {
        public DetailsQueryResult(string category, IEnumerable<string> types)
        {
            Category = category;
            Types = types;
        }

        public string Category { get; private set; }
        public IEnumerable<string> Types { get; private set; }

        public StepsQueryInput GetStepsInput(string type)
        {
            return new StepsQueryInput
            {
                Slug = Category,
                Id = type
            };
        }
    }
}