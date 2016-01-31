using System;
using System.Collections.Generic;
using System.Linq;
using Consul;

namespace SuperGlue.Discovery.Consul
{
    public class ConsulServiceSettings
    {
        private readonly ICollection<string> _tags = new List<string>();
        private readonly ICollection<AgentServiceCheck> _checks = new List<AgentServiceCheck>();
        private readonly ICollection<Action<ConsulClientConfiguration>> _alterSettings = new List<Action<ConsulClientConfiguration>>();

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }

        public ConsulServiceSettings WithId(string id)
        {
            Id = id;

            return this;
        }

        public ConsulServiceSettings WithName(string name)
        {
            Name = name;

            return this;
        }

        public ConsulServiceSettings WithAddress(string address)
        {
            Address = address;

            return this;
        }

        public ConsulServiceSettings WithPort(int port)
        {
            Port = port;

            return this;
        }

        public ConsulServiceSettings WithTag(string tag)
        {
            _tags.Add(tag);

            return this;
        }

        public ConsulServiceSettings WithCheck(AgentServiceCheck check)
        {
            _checks.Add(check);

            return this;
        }

        public ConsulServiceSettings AlterSettingsWith(Action<ConsulClientConfiguration> alter)
        {
            _alterSettings.Add(alter);

            return this;
        }

        public string[] GetTags()
        {
            return _tags.ToArray();
        }

        public AgentServiceCheck[] GetChecks()
        {
            return _checks.ToArray();
        }

        public ConsulClient CreateClient()
        {
            var settings = new ConsulClientConfiguration();

            foreach (var alterSetting in _alterSettings)
                alterSetting(settings);

            return new ConsulClient(settings);
        }
    }
}