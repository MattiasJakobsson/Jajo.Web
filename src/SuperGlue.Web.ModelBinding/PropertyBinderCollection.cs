using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.ModelBinding
{
    public class PropertyBinderCollection : IPropertyBinderCollection
    {
        private readonly IEnumerable<IPropertyBinder> _propertyBinders;

        public PropertyBinderCollection(IEnumerable<IPropertyBinder> propertyBinders)
        {
            _propertyBinders = propertyBinders;
        }

        public IEnumerator<IPropertyBinder> GetEnumerator()
        {
            return _propertyBinders.GetEnumerator();
        }

        public IPropertyBinder GetMatching(PropertyInfo propertyInfo)
        {
            return _propertyBinders.FirstOrDefault(x => x.Matches(propertyInfo));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}