using System.IO;

namespace SuperGlue.Web.Assets
{
    public interface IContainAssetInformation
    {
        string MimeType { get; }
        Stream Asset { get; }
    }
}