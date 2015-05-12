using System;
using System.Collections.Generic;
using Jajo.Web.Configuration;

namespace Jajo.Web.Sample.SubApplication
{
    public class TestSetup : IRunAtConfigurationTime
    {
        public void Configure(IDictionary<string, object> applicationData)
        {
            Console.WriteLine("Test from subapp");
        }
    }
}