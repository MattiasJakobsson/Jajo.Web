using System;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public interface IModelBinder
    {
        bool Matches(Type type);
        Task<BindingResult> Bind(Type type, IBindingContext bindingContext);
    }

    public class BindingResult
    {
        public BindingResult(object instance, bool success)
        {
            Instance = instance;
            Success = success;
        }

        public object Instance { get; private set; }
        public bool Success { get; private set; }
    }
}