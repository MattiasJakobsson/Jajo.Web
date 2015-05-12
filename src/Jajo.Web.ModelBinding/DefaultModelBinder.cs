using System;
using System.Linq;

namespace Jajo.Web.ModelBinding
{
    public class DefaultModelBinder : IModelBinder
    {
        private readonly IPropertyBinderCollection _propertyBinderCollection;

        public DefaultModelBinder(IPropertyBinderCollection propertyBinderCollection)
        {
            _propertyBinderCollection = propertyBinderCollection;
        }

        /// <summary>
        /// Checks if this <see cref="IModelBinder"/> instance can handle the requested type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>A <see cref="bool"/> indicating a match</returns>
        public bool Matches(Type type)
        {
            return true;
        }

        /// <summary>
        /// Creates a new instance of the requested <see cref=" Type"/> and binds it's properties
        /// </summary>
        /// <param name="type">The type to bind</param>
        /// <param name="bindingContext">The context to bind in</param>
        /// <returns>A new binded instance of the requested <see cref="Type"/></returns>
        public object Bind(Type type, IBindingContext bindingContext)
        {
            var instance = Activator.CreateInstance(type);
            return Bind(type, bindingContext, instance) ? instance : null;
        }

        /// <summary>
        /// Binds a existing instance of the requested <see cref="Type"/>
        /// </summary>
        /// <param name="type">The type to bind</param>
        /// <param name="bindingContext">The context to bind in</param>
        /// <param name="instance">The instance to use when binding</param>
        /// <returns>The requested instance with binded properties</returns>
        public bool Bind(Type type, IBindingContext bindingContext, object instance)
        {
            var boundPropertyCount = (from propertyInfo in type.GetProperties().Where(x => x.CanWrite) 
                                      let propertyBinder = _propertyBinderCollection.GetMatching(propertyInfo) 
                                      where propertyBinder != null 
                                      where propertyBinder.Bind(instance, propertyInfo, bindingContext) 
                                      select propertyInfo).Count();

            return boundPropertyCount > 0;
        }
    }
}