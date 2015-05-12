using System;

namespace Jajo.Web.ModelBinding
{
    public interface IModelBinder
    {
        /// <summary>
        /// Checks if this <see cref="IModelBinder"/> instance can handle the requested type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>A <see cref="bool"/> indicating a match</returns>
        bool Matches(Type type);

        /// <summary>
        /// Creates a new instance of the requested <see cref=" Type"/> and binds it's properties
        /// </summary>
        /// <param name="type">The type to bind</param>
        /// <param name="bindingContext">The context to bind in</param>
        /// <returns>A new binded instance of the requested <see cref="Type"/></returns>
        object Bind(Type type, IBindingContext bindingContext);

        /// <summary>
        /// Binds a existing instance of the requested <see cref="Type"/>
        /// </summary>
        /// <param name="type">The type to bind</param>
        /// <param name="bindingContext">The context to bind in</param>
        /// <param name="instance">The instance to use when binding</param>
        /// <returns>A <see cref="bool"/> indicating the success or failure of the binding</returns>
        bool Bind(Type type, IBindingContext bindingContext, object instance);
    }
}