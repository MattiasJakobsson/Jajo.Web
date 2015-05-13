using System;
using System.Collections.Generic;
using SuperGlue.Web.Configuration;

namespace SuperGlue.Web.Sample.SubApplication
{
    public class TestSetup : IRunAtConfigurationTime
    {
        public void Configure(IDictionary<string, object> applicationData)
        {
            Console.WriteLine("Test from subapp");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            Console.WriteLine("Test from subapp. Shutting down!");
        }
    }
}