using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Sample.SubApplication
{
    public class TestSetup : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.TestSetup", x => Console.WriteLine("Test from subapp"));
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            Console.WriteLine("Test from subapp. Shutting down!");
        }
    }
}