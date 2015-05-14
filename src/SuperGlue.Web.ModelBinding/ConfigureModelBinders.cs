using System.Collections.Generic;
using SuperGlue.Configuration;
using SuperGlue.Web.ModelBinding.BindingSources;
using SuperGlue.Web.ModelBinding.PropertyBinders;
using SuperGlue.Web.ModelBinding.ValueConverters;

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