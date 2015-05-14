using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<bool> ContainsKey(string key, IDictionary<string, object> environment)
        {
            return (await GetSourcesContainingKey(key.ToLower(), environment)).Any();
        }

        public async Task<object> Get(string key, IDictionary<string, object> environment)
        {
            var matchingSources = (await GetSourcesContainingKey(key.ToLower(), environment)).ToList();

            if (!matchingSources.Any()) 
                return null;

            var bindingSource = matchingSources.First();

            return (await bindingSource.GetValues(environment))[key.ToLower()];
        }

        private async Task<IEnumerable<IBindingSource>> GetSourcesContainingKey(string key, IDictionary<string, object> environment)
        {
            var results = new List<IBindingSource>();

            foreach (var bindingSource in _bindingSources)
            {
                if ((await bindingSource.GetValues(environment)).ContainsKey(key.ToLower()))
                    results.Add(bindingSource);
            }

            return results;
        }
    }
}