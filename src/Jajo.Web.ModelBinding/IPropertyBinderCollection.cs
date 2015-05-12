using System.Collections.Generic;
using System.Reflection;

namespace Jajo.Web.ModelBinding
{
    public interface IPropertyBinderCollection : IEnumerable<IPropertyBinder>
    {
        IPropertyBinder GetMatching(PropertyInfo propertyInfo);
    }
}