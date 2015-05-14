using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Topshelf
{
    public class SuperGlueServiceRuntime
    {
        private SuperGlueBootstrapper _bootstrapper;

        public void Start()
        {
            _bootstrapper = SuperGlueBootstrapper.Find();

            _bootstrapper.StartApplications();
        }

        public void Stop()
        {
            if(_bootstrapper != null)
                _bootstrapper.ShutDown();
        }
    }
}