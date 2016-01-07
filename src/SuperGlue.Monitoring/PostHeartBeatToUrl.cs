using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SuperGlue.Monitoring
{
    public class PostHeartBeatToUrl : IMonitorHeartBeats
    {
        private readonly string _url;
        private readonly string _message;

        public PostHeartBeatToUrl(string url, string message = null)
        {
            _url = url;
            _message = message;
        }

        public async Task Beat(IDictionary<string, object> environment)
        {
            var webRequest = WebRequest.Create(_url);
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";

            var requestStream = await webRequest.GetRequestStreamAsync();
            var streamWriter = new StreamWriter(requestStream);

            await streamWriter.WriteAsync(_message ?? "");

            await streamWriter.FlushAsync();
            requestStream.Close();

            (await webRequest.GetResponseAsync()).Close();
        }
    }
}