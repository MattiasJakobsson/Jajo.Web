using System.Collections.Generic;
using System.Reflection;

namespace OpenWeb.ModelBinding
{
    public interface IPropertyBinderCollection : IEnumerable<IPropertyBinder>
    {
        IPropertyBinder GetMatching(PropertyInfo propertyInfo);
    }
}