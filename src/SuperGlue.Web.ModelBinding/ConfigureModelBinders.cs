using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Web.ModelBinding.BindingSources;
using SuperGlue.Web.ModelBinding.PropertyBinders;
using SuperGlue.Web.ModelBinding.ValueConverters;

namespace SuperGlue.Web.ModelBinding
{
    public class ConfigureModelBinders : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ModelBinding.Configured", environment =>
            {
                environment.RegisterAll(typeof(IBindingSource));
                environment.RegisterAll(typeof(IPropertyBinder));
                environment.RegisterAll(typeof(IModelBinder));
                environment.RegisterAll(typeof(IValueConverter));

                environment.RegisterSingletonType(typeof(IModelBinderCollection), typeof(ModelBinderCollection));
                environment.RegisterSingletonType(typeof(IPropertyBinderCollection), typeof(PropertyBinderCollection));
                environment.RegisterSingletonType(typeof(IValueConverterCollection), typeof(ValueConverterCollection));
                environment.RegisterSingletonType(typeof(IBindingSourceCollection), typeof(BindingSourceCollection));
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}