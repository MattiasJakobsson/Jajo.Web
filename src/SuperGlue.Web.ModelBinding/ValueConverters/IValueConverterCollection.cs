using System;
using System.Collections.Generic;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public interface IValueConverterCollection : IEnumerable<IValueConverter>
    {
        bool CanConvert(Type destinationType, IBindingContext context);
        BindingResult Convert(Type destinationType, object value, IBindingContext context);
    }
}