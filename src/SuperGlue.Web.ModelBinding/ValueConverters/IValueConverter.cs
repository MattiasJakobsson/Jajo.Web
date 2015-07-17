using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public interface IValueConverter
    {
        bool Matches(Type destinationType);
        BindingResult Convert(Type destinationType, object value);
    }
}