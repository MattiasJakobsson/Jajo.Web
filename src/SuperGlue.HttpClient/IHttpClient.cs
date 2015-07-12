using System;

namespace SuperGlue.HttpClient
{
    public interface IHttpClient
    {
        IHttpRequest Start(Uri url);
    }
}