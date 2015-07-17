using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public class BindingContext : IBindingContext
    {
        private string _currentPrefix;
        private readonly IModelBinderCollection _modelBinderCollection;

        public BindingContext(IModelBinderCollection modelBinderCollection, IDictionary<string, object> environment)
            : this(modelBinderCollection, environment, "")
        {
            
        }

        public BindingContext(IModelBinderCollection modelBinderCollection, IDictionary<string, object> environment, string prefix)
        {
            _currentPrefix = prefix;
            Environment = environment;
            _modelBinderCollection = modelBinderCollection;
        }

        public Task<object> Bind(Type type)
        {
            return _modelBinderCollection.Bind(type, this);
        }

        public void PrefixWith(string prefix)
        {
            _currentPrefix = string.Format("{0}{1}", _currentPrefix, prefix);
        }

        public string GetKey(string name)
        {
            return string.Format("{0}{1}", _currentPrefix, name).ToLower();
        }

        public string GetPrefix()
        {
            return _currentPrefix;
        }

        public IDisposable OpenChildContext(string prefix)
        {
            var oldPrefix = _currentPrefix;
            
            PrefixWith(prefix.ToLower());

            return new Disposable(oldPrefix, x => _currentPrefix = x);
        }

        public IDictionary<string, object> Environment { get; private set; }

        private class Disposable : IDisposable
        {
            private readonly string _oldPrefix;
            private readonly Action<string> _reset;

            public Disposable(string oldPrefix, Action<string> reset)
            {
                _oldPrefix = oldPrefix;
                _reset = reset;
            }

            public void Dispose()
            {
                _reset(_oldPrefix);
            }
        }
    }
}