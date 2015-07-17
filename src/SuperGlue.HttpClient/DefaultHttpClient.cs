using System;
using System.Collections.Generic;

namespace SuperGlue.HttpClient
{
    public class DefaultHttpClient : IHttpClient
    {
        private readonly IEnumerable<IParseContentType> _parseContentTypes;

        public DefaultHttpClient(IEnumerable<IParseContentType> parseContentTypes)
        {
            _parseContentTypes = parseContentTypes;
        }

        public IHttpRequest Start(Uri url)
        {
            return new DefaultHttpRequest(url, _parseContentTypes);
        }
    }
}