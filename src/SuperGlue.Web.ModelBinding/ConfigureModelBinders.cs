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
                environment.RegisterAll(typeof(IBindingSource));
                environment.RegisterAll(typeof(IPropertyBinder));
                environment.RegisterAll(typeof(IModelBinder));
                environment.RegisterAll(typeof(IValueConverter));

                environment.RegisterSingleton(typeof(IModelBinderCollection), typeof(ModelBinderCollection));
                environment.RegisterSingleton(typeof(IPropertyBinderCollection), typeof(PropertyBinderCollection));
                environment.RegisterSingleton(typeof(IValueConverterCollection), typeof(ValueConverterCollection));
                environment.RegisterSingleton(typeof(IBindingSourceCollection), typeof(BindingSourceCollection));
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }
    }
}