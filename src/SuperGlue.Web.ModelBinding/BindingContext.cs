using System;

namespace SuperGlue.Web.ModelBinding
{
    public class BindingContext : IBindingContext
    {
        private string _currentPrefix;
        private readonly IModelBinderCollection _modelBinderCollection;

        public BindingContext(IModelBinderCollection modelBinderCollection)
            : this(modelBinderCollection, string.Empty)
        {
            
        }

        public BindingContext(IModelBinderCollection modelBinderCollection, string prefix)
        {
            _currentPrefix = prefix;
            _modelBinderCollection = modelBinderCollection;
        }

        public object Bind(Type type)
        {
            return _modelBinderCollection.Bind(type, this);
        }

        public void Bind(Type type, object instance)
        {
            _modelBinderCollection.Bind(type, this, instance);
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