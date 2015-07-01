using System;
using System.Collections.Generic;
using System.IO;

namespace SuperGlue.Web.PartialRequests
{
    public static class PartialEnvironmentExtensions
    {
        public static class PartialConstants
        {
            public const string IsPartialRequest = "superglue.IsPartialRequest";
            public const string PartialSettings = "superglue.PartialSettings";
        }

        public static IDictionary<string, object> CreatePartialRequest(this IDictionary<string, object> environment, object partial)
        {
            var partialEnvironment = new Dictionary<string, object>();

            foreach (var item in environment)
                partialEnvironment[item.Key] = item.Value;

            var urlParts = environment.RouteTo(partial).Split('?');

            var responseBody = new MemoryStream();
            partialEnvironment[WebEnvironmentExtensions.OwinConstants.ResponseBody] = responseBody;

            partialEnvironment[WebEnvironmentExtensions.OwinConstants.RequestMethod] = "GET";
            partialEnvironment[WebEnvironmentExtensions.OwinConstants.RequestPath] = urlParts[0];
            partialEnvironment[WebEnvironmentExtensions.OwinConstants.RequestQueryString] = urlParts.Length > 1 ? urlParts[1] : "";

            partialEnvironment[PartialConstants.IsPartialRequest] = true;

            return partialEnvironment;
        }

        public static bool IsPartial(this IDictionary<string, object> environment)
        {
            return environment.Get<bool>(PartialConstants.IsPartialRequest);
        }

        public static bool IsPartialEndpoint(this IDictionary<string, object> environment, object endpoint)
        {
            return environment.Get(PartialConstants.PartialSettings, new PartialSettings()).IsPartialEndpoint(endpoint);
        }
    }
}