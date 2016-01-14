using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public class EmptyEnd : IEndThings
    {
        public Task End()
        {
            return Task.CompletedTask;
        }
    }
}