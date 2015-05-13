using System.Collections.Generic;
using System.Reflection;

namespace SuperGlue.Web.ModelBinding
{
    public interface IPropertyBinderCollection : IEnumerable<IPropertyBinder>
    {
        IPropertyBinder GetMatching(PropertyInfo propertyInfo);
    }
}