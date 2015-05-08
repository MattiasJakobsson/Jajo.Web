using System.IO;
using System.Reflection;

namespace OpenWeb.Endpoints
{
    public interface IOpenWebContext
    {
        IWebEnvironment Environment { get; }
        MethodInfo RoutedTo { get; }
        Stream Body { get; }
        T Get<T>();
        void Set<T>(T data);
        IResolveDependencies DependencyResolver { get; set; }
    }
}