using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SuperGlue.Configuration
{
    [Serializable]
    public class RemoteBootstrapper : MarshalByRefObject
    {
        private SuperGlueBootstrapper _bootstrapper;

        public void Initialize(string path)
        {
            foreach (var dll in Directory.GetFiles(path, "*.dll"))
                Assembly.LoadFile(dll);

            _bootstrapper = SuperGlueBootstrapper.Find();
        }

        public void Start(string environment, IDictionary<string, string[]> hostArguments)
        {
            _bootstrapper.StartApplications(new Dictionary<string, object>(), environment, hostArguments).Wait();
        }

        public void Stop()
        {
            _bootstrapper.ShutDown().Wait();
        }

        public string GetApplicationName()
        {
            return _bootstrapper.ApplicationName;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}