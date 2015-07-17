using System;

namespace SuperGlue.ApiDiscovery
{
    public class ChildSelector
    {
        public ChildSelector(string collection, Func<ApiResource, bool> matcher)
        {
            Collection = collection;
            Matcher = matcher;
        }

        public string Collection { get; private set; }
        public Func<ApiResource, bool> Matcher { get; private set; }
    }
}