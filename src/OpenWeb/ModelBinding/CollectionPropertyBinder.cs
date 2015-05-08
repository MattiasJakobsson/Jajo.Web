using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenWeb.ModelBinding
{
    public class CollectionPropertyBinder : IPropertyBinder
    {
        private static readonly Cache<Type, MethodInfo> AddMethods = new Cache<Type, MethodInfo>();

        public CollectionPropertyBinder()
        {
            AddMethods.OnMissing = type => type.GetMethod("Add");
        }

        public bool Matches(PropertyInfo propertyInfo)
        {
            return typeof (IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);
        }

        public bool Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            var type = propertyInfo.PropertyType;
            var itemType = type.GetGenericArguments()[0];
            if (type.IsInterface)
            {
                type = typeof(List<>).MakeGenericType(itemType);
            }

            var currentCollection = propertyInfo.GetValue(instance, null);
            var collection = currentCollection ?? Activator.CreateInstance(type);
            var collectionType = collection.GetType();

            Func<Type, string, bool> addToCollection = (typeToBind, prefix) =>
                                                           {
                                                               using (bindingContext.OpenChildContext(prefix))
                                                               {
                                                                   var addMethod = AddMethods[collectionType];
                                                                   var obj = bindingContext.Bind(itemType);
                                                                   if (obj != null)
                                                                   {
                                                                       addMethod.Invoke(collection, new[] { obj });
                                                                       return true;
                                                                   }
                                                                   return false;
                                                               }
                                                           };

            var formatString = string.Format("{0}{1}", bindingContext.GetPrefix(), propertyInfo.Name) + "[{0}]_";

            var index = 0;
            string currentPrefix;
            do
            {
                currentPrefix = string.Format(formatString, index);
                index++;
            } while (addToCollection(itemType, currentPrefix));

            propertyInfo.SetValue(instance, collection, null);

            return ((IEnumerable) collection).OfType<object>().Any();
        }
    }
}