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

                environment.RegisterTransient(typeof(IModelBinderCollection), typeof(ModelBinderCollection));
                environment.RegisterTransient(typeof(IPropertyBinderCollection), typeof(PropertyBinderCollection));
                environment.RegisterTransient(typeof(IValueConverterCollection), typeof(ValueConverterCollection));
                environment.RegisterTransient(typeof(IBindingSourceCollection), typeof(BindingSourceCollection));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}