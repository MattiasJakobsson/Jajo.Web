using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperGlue.Configuration
{
    public static class Files
    {
        public static Task<T> ReadAsJson<T>(string path)
        {
            if (!File.Exists(path))
                return Task.FromResult(default(T));

            try
            {
                return File.Open(path, FileMode.Open).ReadAsJson<T>();
            }
            catch (Exception)
            {
                return Task.FromResult(default(T));
            }
        }

        public static Task WriteJsonTo(string path, object data)
        {
            var json = JsonConvert.SerializeObject(data);

            File.WriteAllText(path, json);

            return Task.CompletedTask;
        }
    }
}