using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public interface IValueConverter
    {
        bool Matches(Type destinationType);
        object Convert(Type destinationType, object value);
    }
}