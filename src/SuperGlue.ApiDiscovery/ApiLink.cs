using System;

namespace SuperGlue.ApiDiscovery
{
    public class ApiLink : IApiLink
    {
        public string Rel { get; set; }
        public Uri Href { get; set; }
        public bool Templated { get; set; }
        public string Type { get; set; }
        public Uri Deprecation { get; set; }
        public Uri Profile { get; set; }
        public string Title { get; set; }
        public string HrefLang { get; set; }
    }
}