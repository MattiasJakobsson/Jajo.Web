using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenWeb.ModelBinding
{
    public class BindingSourceCollection : IBindingSourceCollection
    {
        private readonly IEnumerable<IBindingSource> _bindingSources;

        public BindingSourceCollection(IEnumerable<IBindingSource> bindingSources)
        {
            _bindingSources = bindingSources;
        }

        public IEnumerator<IBindingSource> GetEnumerator()
        {
            return _bindingSources.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return GetSourcesContainingKey(key.ToLower()).Any();
        }

        public object Get(string key)
        {
            var matchingSources = GetSourcesContainingKey(key.ToLower()).ToList();

            if (!matchingSources.Any()) return null;

            var bindingSource = matchingSources.First();

            return bindingSource.GetValues()[key.ToLower()];
        }

        public IEnumerable<string> GetKeys()
        {
            var keys = new List<string>();
            foreach (var bindingSource in _bindingSources)
                keys.AddRange(bindingSource.GetKeys());

            return keys;
        }

        private IEnumerable<IBindingSource> GetSourcesContainingKey(string key)
        {
            return _bindingSources.Where(x => x.GetValues().ContainsKey(key.ToLower()));
        }
    }
}