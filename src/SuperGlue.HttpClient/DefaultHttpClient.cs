using System;

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