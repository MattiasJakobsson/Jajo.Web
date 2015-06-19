using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Topshelf
{
    public class SuperGlueServiceRuntime
    {
        private SuperGlueBootstrapper _bootstrapper;

        public void Start(string environment)
        {
            _bootstrapper = SuperGlueBootstrapper.Find();

            _bootstrapper.StartApplications(new Dictionary<string, object>(), environment);
        }

        public void Stop()
        {
            if(_bootstrapper != null)
                _bootstrapper.ShutDown();
        }
    }
}