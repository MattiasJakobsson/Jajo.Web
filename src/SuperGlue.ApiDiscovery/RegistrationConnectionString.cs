using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.ApiDiscovery
{
    public class RegistrationConnectionString
    {
        private readonly IReadOnlyDictionary<string, string> _options; 

        public RegistrationConnectionString(string connectionString)
        {
            _options = connectionString
                .Split(';')
                .Select(x => new ConnectionPart(x))
                .ToDictionary(x => x.Name, x => x.Value);
        }

        public string Name => GetOption("name", "registration");
        public Uri Location => new Uri(GetOption("location"));
        public IEnumerable<string> Accepts => GetOption("accepts").Split(',');

        private string GetOption(string key, string defaultValue = "")
        {
            return _options.ContainsKey(key) ? _options[key] ?? defaultValue : defaultValue;
        }

        private class ConnectionPart
        {
            public ConnectionPart(string part)
            {
                var parts = part.Split('=');

                if (!parts.Any())
                    return;

                Name = parts[0];

                if (parts.Length < 2)
                    return;

                Value = parts[1];
            }

            public string Name { get; }
            public string Value { get; }
        }
    }
}