using System.Collections.Generic;
using System.Reflection;

namespace SuperGlue.Web.Routing.Superscribe.Conventional
{
    public interface ICheckIfRouteExists
    {
        bool Exists(MethodInfo method, IDictionary<string, object> environment);
    }
}