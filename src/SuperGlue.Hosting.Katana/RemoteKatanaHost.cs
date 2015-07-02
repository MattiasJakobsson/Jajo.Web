using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Katana
{
    public class RemoteKatanaHost : MarshalByRefObject
    {
        private SuperGlueBootstrapper _bootstrapper;

        public void Start(string environment)
        {
            _bootstrapper = SuperGlueBootstrapper.Find();

            _bootstrapper.StartApplications(new Dictionary<string, object>(), environment, ApplicationStartersOverrides.Prefer<StartKatanaHost>()).Wait();
        }

        public void Stop()
        {
            if (_bootstrapper != null)
                _bootstrapper.ShutDown().Wait();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}