using System;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public interface IModelBinder
    {
        bool Matches(Type type);
        Task<object> Bind(Type type, IBindingContext bindingContext);
    }
}