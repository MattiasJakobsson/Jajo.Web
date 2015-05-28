using System;
using System.Reflection;

namespace SuperGlue.Web.Routing.Superscribe
{
    public class RouteParameter
    {
        private readonly Func<object, object> _getValue;

        public RouteParameter(string name, Type type, Func<object, object> getValue)
        {
            Name = name;
            _getValue = getValue;
            Type = type;
        }

        public static RouteParameter FromProperty(PropertyInfo property)
        {
            return new RouteParameter(property.Name, property.PropertyType, property.GetValue);
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }

        public object GetValue(object input)
        {
            return _getValue(input);
        }
    }
}