using System;

namespace SuperGlue.Configuration.Ioc
{
    public class GenericInterfaceScanRegistration : IServiceRegistration
    {
        public GenericInterfaceScanRegistration(Type scanType)
        {
            ScanType = scanType;
        }

        public Type ScanType { get; private set; }
    }
}