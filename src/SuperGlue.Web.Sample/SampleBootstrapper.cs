using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Sample
{
    public class SampleBootstrapper : SuperGlueBootstrapper
    {
        protected override Task Configure(string environment)
        {
            return Task.CompletedTask;
        }
    }
}