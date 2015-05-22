using System.Collections.Generic;

namespace SuperGlue.Web.Output
{
    public interface ISuperGlueView
    {
        IDictionary<string, object> Environment { get; } 
    }

    public interface ISuperGlueView<out TModel> : ISuperGlueView
    {
        TModel Model { get; } 
    }
}