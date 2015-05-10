using System;
using System.Collections.Generic;
using OpenWeb.Configuration;

namespace OpenWeb.Sample.SubApplication
{
    public class TestSetup : IRunAtConfigurationTime
    {
        public void Configure(IDictionary<string, object> applicationData)
        {
            Console.WriteLine("Test from subapp");
        }
    }
}