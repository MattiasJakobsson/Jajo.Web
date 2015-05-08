using System.Collections.Generic;

namespace OpenWeb
{
    public interface IWebEnvironment
    {
        string Method { get; }
        string ContentType { get; }
        IReadOnlyDictionary<string, string[]> RawHeaders { get; }
        IReadOnlyDictionary<string, object> RouteParameters { get; }
        object Output { get; }
        T Get<T>(string key);
    }
}