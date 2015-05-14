using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Web.ModelBinding
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

        public bool ContainsKey(string key, IDictionary<string, object> environment)
        {
            return GetSourcesContainingKey(key.ToLower(), environment).Any();
        }

        public object Get(string key, IDictionary<string, object> environment)
        {
            var matchingSources = GetSourcesContainingKey(key.ToLower(), environment).ToList();

            if (!matchingSources.Any()) return null;

            var bindingSource = matchingSources.First();

            return bindingSource.GetValues(environment)[key.ToLower()];
        }

        private IEnumerable<IBindingSource> GetSourcesContainingKey(string key, IDictionary<string, object> environment)
        {
            return _bindingSources.Where(x => x.GetValues(environment).ContainsKey(key.ToLower()));
        }
    }
}