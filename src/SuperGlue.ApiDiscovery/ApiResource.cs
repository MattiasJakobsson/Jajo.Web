using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.ApiDiscovery
{
    public class ApiResource
    {
        private readonly Func<Type, object> _convertState;

        public ApiResource(string name, IEnumerable<ApiForm> forms, IEnumerable<ApiResource> children, IEnumerable<ApiLink> links, ApiDefinition definition, Func<Type, object> convertState)
        {
            Links = links;
            Definition = definition;
            _convertState = convertState;
            Name = name;
            Forms = forms.ToDictionary(x => x.Name, x => x);
            Children = children.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.OfType<ApiResource>());
        }

        public string Name { get; }
        public IEnumerable<ApiLink> Links { get; private set; }
        public IReadOnlyDictionary<string, ApiForm> Forms { get; private set; }
        public IReadOnlyDictionary<string, IEnumerable<ApiResource>> Children { get; private set; }
        public ApiDefinition Definition { get; private set; }

        public T StateAs<T>() where T : class
        {
            return _convertState(typeof (T)) as T;
        }
    }
}