using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Assets
{
    public interface ILocateAssets
    {
        Task<IEnumerable<Asset>> FindAssets(IDictionary<string, object> environment);
    }
}