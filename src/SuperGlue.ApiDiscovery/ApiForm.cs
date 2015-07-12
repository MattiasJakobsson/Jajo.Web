using System;
using System.Collections.Generic;

namespace SuperGlue.ApiDiscovery
{
    public class ApiForm : IApiForm
    {
        public string Name { get; set; }
        public Uri Action { get; set; }
        public string Method { get; set; }
        public IReadOnlyDictionary<string, string> Headers { get; set; }
        public IReadOnlyDictionary<string, object> Schema { get; set; }
        public bool Templated { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
    }
}