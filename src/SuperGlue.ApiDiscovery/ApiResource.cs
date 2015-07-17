using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.ApiDiscovery
{
    public class ApiResource
    {
        public ApiResource(string name, dynamic state, IEnumerable<ApiForm> forms, IEnumerable<ApiResource> children, IEnumerable<ApiLink> links, ApiDefinition definition)
        {
            Links = links;
            Definition = definition;
            Name = name;
            State = state;
            Forms = forms.ToDictionary(x => x.Name, x => x);
            Children = children.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.OfType<ApiResource>());
        }

        public string Name { get; private set; }
        public IEnumerable<ApiLink> Links { get; private set; }
        public dynamic State { get; private set; }
        public IReadOnlyDictionary<string, ApiForm> Forms { get; private set; }
        public IReadOnlyDictionary<string, IEnumerable<ApiResource>> Children { get; private set; }
        public ApiDefinition Definition { get; private set; }
    }
}