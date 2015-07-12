using System.Collections.Generic;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiResource
    {
        ApiDefinition Definition { get; }
        string Name { get; }
        IEnumerable<IApiLink> Links { get; }
        IReadOnlyDictionary<string, StateObject> State { get; }
        IReadOnlyDictionary<string, IApiForm> Forms { get; } 
        IReadOnlyDictionary<string, IEnumerable<IApiResource>> Children { get; }
    }
}