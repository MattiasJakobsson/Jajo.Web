using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output.Html.Autocomplete
{
    public class Search
    {
        private readonly IEnumerable<IAutocompleteSearcher> _searchers;

        public Search(IEnumerable<IAutocompleteSearcher> searchers)
        {
            _searchers = searchers;
        }

        public async Task<SearchQueryResult> SearchQuery(SearchQueryInput input)
        {
            var searcher = _searchers.FirstOrDefault(x => x.GetName().Equals(input.Slug, StringComparison.InvariantCultureIgnoreCase));

            return new SearchQueryResult(searcher == null ? new List<AutocompleteSearchResult>() : await searcher.Search(input.Search));
        }
    }

    public class SearchQueryInput
    {
        public string Search { get; set; }
        public string Slug { get; set; }
    }

    public class SearchQueryResult
    {
        public SearchQueryResult(IEnumerable<AutocompleteSearchResult> results)
        {
            Results = results;
        }

        public IEnumerable<AutocompleteSearchResult> Results { get; private set; }
    }
}