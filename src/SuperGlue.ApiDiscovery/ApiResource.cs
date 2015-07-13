using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.ApiDiscovery
{
    public class ApiResource : IApiResource
    {
        public ApiResource(string name, dynamic state, IEnumerable<IApiForm> forms, IEnumerable<IApiResource> children, IEnumerable<IApiLink> links)
        {
            Links = links;
            Name = name;
            State = state;
            Forms = forms.ToDictionary(x => x.Name, x => x);
            Children = children.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.OfType<IApiResource>());
        }

        public string Name { get; private set; }
        public IEnumerable<IApiLink> Links { get; private set; }
        public dynamic State { get; private set; }
        public IReadOnlyDictionary<string, IApiForm> Forms { get; private set; }
        public IReadOnlyDictionary<string, IEnumerable<IApiResource>> Children { get; private set; }
    }
}