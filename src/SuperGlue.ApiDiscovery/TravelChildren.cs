using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.ApiDiscovery
{
    public class TravelChildren : IApiLinkTravelInstruction
    {
        private readonly string _rel;
        private readonly IEnumerable<ChildSelector> _childSelectors;

        public TravelChildren(string rel, params ChildSelector[] childSelectors)
        {
            _rel = rel;
            _childSelectors = childSelectors;
        }

        public IApiLink TravelTo(IApiResource resource)
        {
            var currentResource = resource;

            foreach (var selector in _childSelectors)
            {
                if (currentResource == null)
                    return null;

                if (!currentResource.Children.ContainsKey(selector.Collection))
                    return null;

                currentResource = currentResource.Children[selector.Collection].FirstOrDefault(x => selector.Matcher(x));
            }

            return currentResource != null ? currentResource.Links.FirstOrDefault(x => x.Rel == _rel) : null;
        }
    }
}