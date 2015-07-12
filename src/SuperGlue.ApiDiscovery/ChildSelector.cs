using System;

namespace SuperGlue.ApiDiscovery
{
    public class ChildSelector
    {
        public ChildSelector(string collection, Func<IApiResource, bool> matcher)
        {
            Collection = collection;
            Matcher = matcher;
        }

        public string Collection { get; private set; }
        public Func<IApiResource, bool> Matcher { get; private set; }
    }
}