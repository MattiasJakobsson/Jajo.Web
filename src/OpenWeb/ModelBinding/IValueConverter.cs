using System;

namespace OpenWeb.ModelBinding
{
    public interface IValueConverter
    {
        bool Matches(Type destinationType);
        object Convert(Type destinationType, object value);
    }
}