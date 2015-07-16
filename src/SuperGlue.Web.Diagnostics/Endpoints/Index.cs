using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Diagnostics;

namespace SuperGlue.Web.Diagnostics.Endpoints
{
    public class Index
    {
        private readonly IManageDiagnosticsInformation _manageDiagnosticsInformation;

        public Index(IManageDiagnosticsInformation manageDiagnosticsInformation)
        {
            _manageDiagnosticsInformation = manageDiagnosticsInformation;
        }

        public async Task<IndexQueryResult> Query(IndexQueryInput input)
        {
            var types = await _manageDiagnosticsInformation.GetTypes();

            return new IndexQueryResult(types);
        }
    }

    public class IndexQueryInput
    {

    }

    public class IndexQueryResult
    {
        public IndexQueryResult(IEnumerable<string> types)
        {
            Types = types;
        }

        public IEnumerable<string> Types { get; private set; }

        public DetailsQueryInput GetDetailsInput(string type)
        {
            return new DetailsQueryInput
            {
                Slug = type
            };
        }
    }
}