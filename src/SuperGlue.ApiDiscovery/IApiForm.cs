using System;
using System.Collections.Generic;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiForm
    {
        string Name { get; }
        Uri Action { get; }
        string Method { get; }
        IReadOnlyDictionary<string, string> Headers { get; }
        IReadOnlyDictionary<string, object> Schema { get; }
        bool Templated { get; }
        string Type { get; }
        string Title { get; }
    }
}