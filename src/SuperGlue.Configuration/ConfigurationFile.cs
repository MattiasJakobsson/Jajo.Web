using System.IO;
using Newtonsoft.Json;

namespace SuperGlue.Configuration
{
    public static class ConfigurationFile
    {
        public static TConfig Read<TConfig>(string path, TConfig defaultConfig = null) where TConfig : class, new()
        {
            defaultConfig = defaultConfig ?? new TConfig();

            return !File.Exists(path) ? defaultConfig : JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(path)) ?? defaultConfig;
        }
    }
}