using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Routing
{
    public class EndpointInformation
    {
        public EndpointInformation(object destination, IEnumerable<IUrlPart> urlParts, IDictionary<Type, Func<object, IDictionary<string, object>>> routedParameters, string[] httpMethods)
        {
            HttpMethods = httpMethods;
            RoutedParameters = routedParameters;
            Destination = destination;
            UrlParts = urlParts;
        }

        public object Destination { get; private set; }
        public IDictionary<Type, Func<object, IDictionary<string, object>>> RoutedParameters { get; private set; }
        public IEnumerable<IUrlPart> UrlParts { get; private set; }
        public string[] HttpMethods { get; private set; }
    }
}