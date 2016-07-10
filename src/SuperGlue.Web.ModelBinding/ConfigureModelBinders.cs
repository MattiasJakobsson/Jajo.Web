using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;
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
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IModelBinderCollection), typeof(ModelBinderCollection))
                    .Register(typeof(IPropertyBinderCollection), typeof(PropertyBinderCollection))
                    .Register(typeof(IValueConverterCollection), typeof(ValueConverterCollection))
                    .Register(typeof(IBindingSourceCollection), typeof(BindingSourceCollection))
                    .Scan(typeof(IBindingSource))
                    .Scan(typeof(IPropertyBinder))
                    .Scan(typeof(IModelBinder))
                    .Scan(typeof(IValueConverter)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}