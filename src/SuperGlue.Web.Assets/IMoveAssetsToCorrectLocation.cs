using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Assets
{
    public interface IMoveAssetsToCorrectLocation
    {
        Task Move(IEnumerable<Asset> assets, IDictionary<string, object> environment);
    }
}