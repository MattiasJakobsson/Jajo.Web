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
            var categories = await _manageDiagnosticsInformation.GetCategories();

            return new IndexQueryResult(categories);
        }
    }

    public class IndexQueryInput
    {

    }

    public class IndexQueryResult
    {
        public IndexQueryResult(IEnumerable<string> categories)
        {
            Categories = categories;
        }

        public IEnumerable<string> Categories { get; private set; }

        public DetailsQueryInput GetDetailsInput(string category)
        {
            return new DetailsQueryInput
            {
                Slug = category
            };
        }
    }
}