using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.ApiDiscovery
{
    public class TravelToChildForm : IFormTravelInstruction
    {
        private readonly string _name;
        private readonly IEnumerable<ChildSelector> _childSelectors;

        public TravelToChildForm(string name, params ChildSelector[] childSelectors)
        {
            _name = name;
            _childSelectors = childSelectors;
        }

        public IApiForm TravelTo(IApiResource resource)
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

            return currentResource != null && currentResource.Forms.ContainsKey(_name) ? currentResource.Forms[_name] : null;
        }
    }
}