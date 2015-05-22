using System;
using System.Collections.Generic;
using System.Linq;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Katana
{
    public class RemoteKatanaHost : MarshalByRefObject
    {
        private SuperGlueBootstrapper _bootstrapper;

        public IEnumerable<string> Start()
        {
            _bootstrapper = SuperGlueBootstrapper.Find();

            return _bootstrapper.StartApplications(new Dictionary<string, object>(), ApplicationStartersOverrides.Prefer<StartKatanaHost>()).ToList();
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