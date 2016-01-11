using System;
using System.Collections.Generic;
using System.Reflection;

namespace SuperGlue.Configuration
{
    [Serializable]
    public class RemoteBootstrapper : MarshalByRefObject
    {
        private SuperGlueBootstrapper _bootstrapper;

        public void Initialize(string path)
        {
            Assembly.LoadFrom(path);

            _bootstrapper = SuperGlueBootstrapper.Find();
        }

        public void Start(string environment)
        {
            _bootstrapper.StartApplications(new Dictionary<string, object>(), environment).Wait();
        }

        public void Stop()
        {
            _bootstrapper.ShutDown().Wait();
        }

        public string GetApplicationName()
        {
            return _bootstrapper.ApplicationName;
        }
    }
}