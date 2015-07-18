using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Diagnostics;

namespace SuperGlue.Web.Diagnostics.Endpoints
{
    public class Steps
    {
        private readonly IManageDiagnosticsInformation _manageDiagnosticsInformation;

        public Steps(IManageDiagnosticsInformation manageDiagnosticsInformation)
        {
            _manageDiagnosticsInformation = manageDiagnosticsInformation;
        }

        public async Task<StepsQueryResult> Query(StepsQueryInput input)
        {
            var steps = await _manageDiagnosticsInformation.GetStepsFor(input.Slug, input.Id);

            return new StepsQueryResult(input.Slug, input.Id, steps);
        }
    }

    public class StepsQueryInput
    {
        public string Slug { get; set; }
        public string Id { get; set; }
    }

    public class StepsQueryResult
    {
        public StepsQueryResult(string category, string type, IEnumerable<string> steps)
        {
            Category = category;
            Type = type;
            Steps = steps;
        }

        public string Category { get; private set; }
        public string Type { get; private set; }
        public IEnumerable<string> Steps { get; private set; }

        public DataQueryInput GetDataInput(string step)
        {
            return new DataQueryInput
            {
                Slug = Category,
                Id = Type,
                Step = step
            };
        }
    }
}