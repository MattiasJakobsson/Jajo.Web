using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output.Html.Autocomplete
{
    public interface IAutocompleteSearcher
    {
        string GetName();
        Task<IEnumerable<AutocompleteSearchResult>> Search(string search);
    }
}