using SuperGlue.Web.Output;

namespace SuperGlue.Web.ApiDiscovery
{
    public interface IRenderApiOutput : IRenderOutput
    {
         string Type { get; }
    }
}