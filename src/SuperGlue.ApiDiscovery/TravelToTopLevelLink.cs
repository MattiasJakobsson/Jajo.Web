using System.Linq;

namespace SuperGlue.ApiDiscovery
{
    public class TravelToTopLevelLink : IApiLinkTravelInstruction
    {
        private readonly string _rel;

        public TravelToTopLevelLink(string rel)
        {
            _rel = rel;
        }

        public ApiLink TravelTo(ApiResource resource)
        {
            return resource.Links.FirstOrDefault(x => x.Rel == _rel);
        }
    }
}