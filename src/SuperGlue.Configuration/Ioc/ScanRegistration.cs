using System;

namespace SuperGlue.Configuration.Ioc
{
    public class ScanRegistration : IServiceRegistration
    {
        public ScanRegistration(Type scanType)
        {
            ScanType = scanType;
        }

        public Type ScanType { get; private set; }
    }
}