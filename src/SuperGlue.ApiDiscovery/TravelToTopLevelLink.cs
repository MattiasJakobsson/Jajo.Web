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

        public IApiLink TravelTo(IApiResource resource)
        {
            return resource.Links.FirstOrDefault(x => x.Rel == _rel);
        }
    }
}