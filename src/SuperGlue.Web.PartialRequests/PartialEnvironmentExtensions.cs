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
        }

        public static IDictionary<string, object> CreatePartialRequest(this IDictionary<string, object> environment, object partial)
        {
            var partialEnvironment = new Dictionary<string, object>();

            foreach (var item in environment)
                partialEnvironment[item.Key] = item.Value;

            var url = new Uri(environment.RouteTo(partial));

            var responseBody = new MemoryStream();
            partialEnvironment[WebEnvironmentExtensions.OwinConstants.ResponseBody] = responseBody;

            partialEnvironment[WebEnvironmentExtensions.OwinConstants.RequestMethod] = "GET";
            partialEnvironment[WebEnvironmentExtensions.OwinConstants.RequestPath] = url.LocalPath;
            partialEnvironment[WebEnvironmentExtensions.OwinConstants.RequestQueryString] = url.Query;

            partialEnvironment[PartialConstants.IsPartialRequest] = true;

            return partialEnvironment;
        }

        public static bool IsPartial(this IDictionary<string, object> environment)
        {
            return environment.Get<bool>(PartialConstants.IsPartialRequest);
        }
    }
}