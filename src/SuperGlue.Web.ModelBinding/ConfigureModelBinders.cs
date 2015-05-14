using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Web.ModelBinding
{
    public class ConfigureModelBinders : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.ModelBinding.Configured", environment =>
            {
                var registerAll = environment.Get<Action<Type>>("superglue.Container.RegisterAll");
                var registerSingleton = environment.Get<Action<Type, Type>>("superglue.Container.RegisterSingletonType");

                registerAll(typeof(IBindingSource));
                registerAll(typeof(IPropertyBinder));
                registerAll(typeof(IModelBinder));
                registerAll(typeof(IValueConverter));

                registerSingleton(typeof(IModelBinderCollection), typeof(ModelBinderCollection));
                registerSingleton(typeof(IPropertyBinderCollection), typeof(PropertyBinderCollection));
                registerSingleton(typeof(IValueConverterCollection), typeof(ValueConverterCollection));
                registerSingleton(typeof(IBindingSourceCollection), typeof(BindingSourceCollection));
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }
    }
}