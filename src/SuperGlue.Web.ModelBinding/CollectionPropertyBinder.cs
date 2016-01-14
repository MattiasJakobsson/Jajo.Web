using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Web.ModelBinding.PropertyBinders;

namespace SuperGlue.Web.ModelBinding
{
    public class CollectionPropertyBinder : IPropertyBinder
    {
        private static readonly Cache<Type, MethodInfo> AddMethods = new Cache<Type, MethodInfo>();

        public CollectionPropertyBinder()
        {
            AddMethods.OnMissing = type => type.GetMethod("Add");
        }

        public bool Matches(PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            return typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.PropertyType != typeof(string);
        }

        public async Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
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

            Func<Type, string, Task<bool>> addToCollection = async (typeToBind, prefix) =>
                                                           {
                                                               using (bindingContext.OpenChildContext(prefix))
                                                               {
                                                                   var addMethod = AddMethods[collectionType];
                                                                   var result = await bindingContext.Bind(itemType).ConfigureAwait(false);

                                                                   if (!result.Success) return false;
                                                                   
                                                                   addMethod.Invoke(collection, new[] { result.Instance });

                                                                   return true;
                                                               }
                                                           };

            var formatString = string.Concat(propertyInfo.Name, "[{0}]_");

            var index = 0;
            string currentPrefix;
            do
            {
                currentPrefix = string.Format(formatString, index);
                index++;
            } while (await addToCollection(itemType, currentPrefix).ConfigureAwait(false));

            propertyInfo.SetValue(instance, collection, null);

            return ((IEnumerable)collection).OfType<object>().Any();
        }
    }
}