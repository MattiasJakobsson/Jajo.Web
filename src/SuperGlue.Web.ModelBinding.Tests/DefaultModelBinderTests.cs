using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Should;
using SuperGlue.Web.ModelBinding.BindingSources;
using SuperGlue.Web.ModelBinding.PropertyBinders;
using SuperGlue.Web.ModelBinding.ValueConverters;

namespace SuperGlue.Web.ModelBinding.Tests
{
    public class DefaultModelBinderTests : ModelBindingTests
    {
        public void when_binding_simple_type_correct_data_should_be_bound()
        {
            var model = Bind<SimpleModel>();

            model.ShouldNotBeNull();
            model.Name.ShouldEqual("Mattias");
        }

        public void when_binding_complex_type_correct_data_should_be_bound()
        {
            var model = Bind<ComplexModel>();

            model.ShouldNotBeNull();
            model.SimpleModel.ShouldNotBeNull();
            model.SimpleModel.Name.ShouldEqual("Jakobsson");
        }

        public void when_binding_complex_type_with_list_correct_data_should_be_bound()
        {
            var model = Bind<ComplexModelWithList>();

            model.ShouldNotBeNull();
            model.Data.ShouldNotBeNull();
            model.Data.Count().ShouldEqual(2);
            model.Data.ElementAt(0).ShouldEqual("Mattias1");
            model.Data.ElementAt(1).ShouldEqual("Mattias2");
        }

        public void when_binding_complex_type_with_types_list_correct_data_should_be_bound()
        {
            var model = Bind<ComplexModelWithTypesList>();

            model.ShouldNotBeNull();
            model.SimpleModels.ShouldNotBeNull();
            model.SimpleModels.Count().ShouldEqual(2);
            model.SimpleModels.ElementAt(0).Data.Count().ShouldEqual(2);
            model.SimpleModels.ElementAt(0).Data.ElementAt(0).ShouldEqual("Jakobsson1");
            model.SimpleModels.ElementAt(0).Data.ElementAt(1).ShouldEqual("Jakobsson2");
            model.SimpleModels.ElementAt(1).Data.Count().ShouldEqual(1);
            model.SimpleModels.ElementAt(1).Data.ElementAt(0).ShouldEqual("Jakobsson3");
        }

        public class SimpleModel
        {
            public string Name { get; set; }
        }

        public class ComplexModel
        {
            public SimpleModel SimpleModel { get; set; }
        }

        public class ComplexModelWithList
        {
            public IEnumerable<string> Data { get; set; }
        }

        public class ComplexModelWithTypesList
        {
            public IEnumerable<ComplexModelWithList> SimpleModels { get; set; }
        }

        protected override IEnumerable<IBindingSource> GetBindingSources()
        {
            yield return new FakeBindingSource(new Dictionary<string, object>
            {
                {"Name", "Mattias"},
                {"SimpleModel_Name", "Jakobsson"},
                {"Data[0]_", "Mattias1"},
                {"Data[1]_", "Mattias2"},
                {"SimpleModels[0]_Data[0]_", "Jakobsson1"},
                {"SimpleModels[0]_Data[1]_", "Jakobsson2"},
                {"SimpleModels[1]_Data[0]_", "Jakobsson3"}
            });
        }
    }

    public abstract class ModelBindingTests
    {
        protected virtual T Bind<T>() where T : class
        {
            var modelBinderCollection = GetModelBinderCollection();

            return modelBinderCollection.Bind(typeof(T), new BindingContext(modelBinderCollection, new Dictionary<string, object>())).Result.Instance as T;
        }

        protected virtual IModelBinderCollection GetModelBinderCollection()
        {
            return new ModelBinderCollection(GetModelBinders(), GetValueConverterCollection(), GetBindingSourceCollection());
        }

        protected virtual IEnumerable<IModelBinder> GetModelBinders()
        {
            yield return new DefaultModelBinder(GetPropertyBinderCollection());
        }

        protected virtual IPropertyBinderCollection GetPropertyBinderCollection()
        {
            return new PropertyBinderCollection(GetPropertyBinders());
        }

        protected virtual IEnumerable<IPropertyBinder> GetPropertyBinders()
        {
            yield return new CollectionPropertyBinder();
            yield return new ComplexTypePropertyBinder(GetValueConverterCollection());
            yield return new SimpleTypePropertyBinder(GetValueConverterCollection(), GetBindingSourceCollection());
        }

        protected virtual IValueConverterCollection GetValueConverterCollection()
        {
            return new ValueConverterCollection(GetValueConverters());
        }

        protected virtual IEnumerable<IValueConverter> GetValueConverters()
        {
            yield return new BoolValueConverter();
            yield return new ByteValueConverter();
            yield return new CharValueConverter();
            yield return new DateTimeValueConverter();
            yield return new DecimalValueConverter();
            yield return new DoubleValueConverter();
            yield return new FloatValueConverter();
            yield return new IntValueConverter();
            yield return new LongValueConverter();
            yield return new SByteValueConverter();
            yield return new ShortValueConverter();
            yield return new StringValueConverter();
            yield return new TimeSpanValueConverter();
            yield return new UIntValueConverter();
            yield return new ULongValueConverter();
            yield return new UShortValueConverter();
            yield return new UriValueConverter();
        }

        protected virtual IBindingSourceCollection GetBindingSourceCollection()
        {
            return new BindingSourceCollection(GetBindingSources());
        }

        protected virtual IEnumerable<IBindingSource> GetBindingSources()
        {
            yield return new FakeBindingSource(new Dictionary<string, object>());
        }

        public class FakeBindingSource : IBindingSource
        {
            private readonly IDictionary<string, object> _data;

            public FakeBindingSource(IDictionary<string, object> data)
            {
                _data = data.ToDictionary(x => x.Key.ToLower(), x => x.Value);
            }

            public Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
            {
                return Task.FromResult(_data);
            }
        }
    }
}
