using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Katana
{
    public class RemoteHost : MarshalByRefObject
    {
        private SuperGlueBootstrapper _bootstrapper;

        public void Start()
        {
            _bootstrapper = SuperGlueBootstrapper.Find();

            _bootstrapper.StartApplications(new Dictionary<string, object>(), ApplicationStartersOverrides.Prefer<StartKatanaHost>());
        }

        public void Stop()
        {
            if (_bootstrapper != null)
                _bootstrapper.ShutDown();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}