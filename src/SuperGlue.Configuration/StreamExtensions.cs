using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperGlue.Configuration
{
    public static class StreamExtensions
    {
        public static async Task<T> ReadAsJson<T>(this Stream stream)
        {
            try
            {
                if (stream == null)
                    return default(T);

                stream.Position = 0;
                
                using (var reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync().ConfigureAwait(false));
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}