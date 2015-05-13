using System;
using System.Collections.Generic;

namespace SuperGlue.Web.ModelBinding
{
    public class DefaultRequestBinder : IRequestBinder
    {
        private readonly IDictionary<Type, object> _bindedTypes = new Dictionary<Type, object>();

        private readonly IModelBinderCollection _modelBinderCollection;

        public DefaultRequestBinder(IModelBinderCollection modelBinderCollection)
        {
            _modelBinderCollection = modelBinderCollection;
        }

        public T Get<T>()
        {
            var value = Get(typeof(T));
            if (value == null) return default(T);

            return (T)value;
        }

        public object Get(Type type)
        {
            lock (_bindedTypes)
            {
                if (_bindedTypes.ContainsKey(type))
                    return _bindedTypes[type];
            }

            return _modelBinderCollection.Bind(type, new BindingContext(_modelBinderCollection));
        }

        public void Set<T>(T input)
        {
            lock (_bindedTypes)
            {
                if (_bindedTypes.ContainsKey(typeof(T)))
                {
                    _bindedTypes[typeof(T)] = input;
                    return;
                }

                _bindedTypes.Add(typeof(T), input);
            }
        }
    }
}