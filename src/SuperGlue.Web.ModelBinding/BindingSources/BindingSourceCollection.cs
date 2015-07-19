using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.BindingSources
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
            var result = (await GetSourcesContainingKey(key.ToLower(), environment)).Any();

            environment.Log("Searched for binding key: {0} with result: Success = {1}.", key, result);

            return result;
        }

        public async Task<object> Get(string key, IDictionary<string, object> environment)
        {
            environment.Log("Searching for binding key: {0}", LogLevel.Debug, key);

            var matchingSources = (await GetSourcesContainingKey(key.ToLower(), environment)).ToList();

            if (!matchingSources.Any())
            {
                environment.Log("Failed to find any matching source for binding key: {0}", LogLevel.Debug, key);
                return null;
            }

            var bindingSource = matchingSources.First();

            var result = (await bindingSource.GetValues(environment))[key.ToLower()];

            environment.Log("Binding key: {0} with value: {1} using source: {2}", LogLevel.Debug, key, result ?? "null", bindingSource);

            return result;
        }

        private async Task<IEnumerable<IBindingSource>> GetSourcesContainingKey(string key, IDictionary<string, object> environment)
        {
            var results = new List<IBindingSource>();

            foreach (var bindingSource in _bindingSources)
            {
                if ((await bindingSource.GetValues(environment)).ContainsKey(key.ToLower()))
                    results.Add(bindingSource);
            }

            environment.Log("Found {0} sources containing key: {1}. \"{2}\"", LogLevel.Debug, results.Count, key, string.Join(", ", results.Select(x => x.GetType().Name)));

            return results;
        }
    }
}