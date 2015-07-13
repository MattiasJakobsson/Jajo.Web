using System.Collections.Generic;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiResource
    {
        string Name { get; }
        IEnumerable<IApiLink> Links { get; }
        dynamic State { get; }
        IReadOnlyDictionary<string, IApiForm> Forms { get; } 
        IReadOnlyDictionary<string, IEnumerable<IApiResource>> Children { get; }
    }
}