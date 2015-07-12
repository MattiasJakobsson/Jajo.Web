using System;
using System.Net.Cache;
using System.Net.Http;

namespace SuperGlue.HttpClient
{
    public class DefaultHttpClient : IHttpClient
    {
        public IHttpRequest Start(Uri url)
        {
            return new DefaultHttpRequest(url);
        }
    }
}