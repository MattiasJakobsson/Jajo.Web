using System;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiLink
    {
        string Rel { get; }
        Uri Href { get; }
        bool Templated { get; }
        string Type { get; }
        Uri Deprecation { get; }
        Uri Profile { get; }
        string Title { get; }
        string HrefLang { get; }
    }
}